window.ScanQRCodeJS = {
    _scanGeneration: 0,
    html5Qrcode: undefined,
    _scanContext: null,
    _controlsBound: false,
    _handlers: {},

    getQrboxSize: function () {
        return (window.innerWidth < 376) ? 250 :
            (window.innerWidth < 475) ? 280 :
                (window.innerWidth < 768) ? 330 : 400;
    },

    getScanConfig: function () {
        var size = this.getQrboxSize();
        return {
            fps: 10,
            qrbox: { width: size, height: size },
            rememberLastUsedCamera: true,
            aspectRatio: 1.0
        };
    },

    stopCameraTracks: function () {
        document.querySelectorAll("video").forEach(function (video) {
            var stream = video.srcObject;
            if (stream && typeof stream.getTracks === "function") {
                stream.getTracks().forEach(function (track) {
                    track.stop();
                });
                video.srcObject = null;
            }
        });
    },

    ToggleLoading: function (isloading) {
        const loading = document.getElementById('app-loading');
        const className = 'al-active';
        if (loading) {
            if (isloading) {
                if (!loading.classList.contains(className)) {
                    loading.classList.add(className);
                }
            } else {
                if (loading.classList.contains(className)) {
                    loading.classList.remove(className);
                }
            }
        }
    },

    _setControlsState: function (state) {
        var startBtn = document.getElementById("qr-start-btn");
        var stopBtn = document.getElementById("qr-stop-btn");
        var fileBtn = document.getElementById("qr-file-btn");
        if (!startBtn || !stopBtn) {
            return;
        }

        var scanning = state === "scanning";
        var initializing = state === "initializing";

        startBtn.disabled = scanning || initializing;
        stopBtn.disabled = !scanning;
        if (fileBtn) {
            fileBtn.disabled = initializing;
        }
    },

    ensureScannerControls: function () {
        var host = document.getElementById("qr-scanner-shell");
        if (!host) {
            return;
        }

        var controls = document.getElementById("qr-scanner-controls");
        if (!controls) {
            controls = document.createElement("div");
            controls.id = "qr-scanner-controls";
            controls.className = "qr-scanner-controls";
            controls.innerHTML =
                '<button type="button" id="qr-start-btn" class="qr-scanner-btn qr-scanner-btn-primary">Start scanning</button>' +
                '<button type="button" id="qr-stop-btn" class="qr-scanner-btn" disabled>Stop scanning</button>' +
                '<button type="button" id="qr-file-btn" class="qr-scanner-btn qr-scanner-btn-secondary">Scan an image file</button>' +
                '<input type="file" id="qr-file-input" accept="image/*" class="qr-scanner-file-input" />' +
                '<p id="qr-scanner-status" class="qr-scanner-status" aria-live="polite"></p>';
            host.appendChild(controls);
        }

        if (this._controlsBound) {
            return;
        }

        var self = this;
        this._handlers.startClick = function () {
            self.startScanning();
        };
        this._handlers.stopClick = function () {
            self.stopScanning();
        };
        this._handlers.fileClick = function () {
            self.BrowseClick();
        };
        this._handlers.fileChange = function (event) {
            self._onFileSelected(event);
        };

        document.getElementById("qr-start-btn").addEventListener("click", this._handlers.startClick);
        document.getElementById("qr-stop-btn").addEventListener("click", this._handlers.stopClick);
        document.getElementById("qr-file-btn").addEventListener("click", this._handlers.fileClick);
        document.getElementById("qr-file-input").addEventListener("change", this._handlers.fileChange);

        this._controlsBound = true;
    },

    _unbindControls: function () {
        var startBtn = document.getElementById("qr-start-btn");
        var stopBtn = document.getElementById("qr-stop-btn");
        var fileBtn = document.getElementById("qr-file-btn");
        var fileInput = document.getElementById("qr-file-input");

        if (startBtn && this._handlers.startClick) {
            startBtn.removeEventListener("click", this._handlers.startClick);
        }
        if (stopBtn && this._handlers.stopClick) {
            stopBtn.removeEventListener("click", this._handlers.stopClick);
        }
        if (fileBtn && this._handlers.fileClick) {
            fileBtn.removeEventListener("click", this._handlers.fileClick);
        }
        if (fileInput && this._handlers.fileChange) {
            fileInput.removeEventListener("change", this._handlers.fileChange);
        }

        delete this._handlers.startClick;
        delete this._handlers.stopClick;
        delete this._handlers.fileClick;
        delete this._handlers.fileChange;
        this._controlsBound = false;
    },

    _setStatus: function (message) {
        var status = document.getElementById("qr-scanner-status");
        if (status) {
            status.textContent = message || "";
        }
    },

    _isScanningState: function (scanner) {
        if (!scanner || typeof scanner.getState !== "function") {
            return false;
        }
        var state = scanner.getState();
        return state === Html5QrcodeScannerState.SCANNING ||
            state === Html5QrcodeScannerState.PAUSED;
    },

    _startCamera: async function (scanGeneration) {
        if (!this.html5Qrcode || !this._scanContext) {
            return false;
        }
        if (scanGeneration !== undefined && scanGeneration !== this._scanGeneration) {
            return false;
        }
        if (this._isScanningState(this.html5Qrcode)) {
            this._setControlsState("scanning");
            return true;
        }

        await setTimeout(async () => {

            var ctx = this._scanContext;
            this._setControlsState("initializing");
            this._setStatus("Starting camera...");

            try {
            await this.html5Qrcode.start(
                { facingMode: "environment" },
                this.getScanConfig(),
                ctx.onScanSuccess,
                ctx.onErrorScan
            );

            if (scanGeneration !== undefined && scanGeneration !== this._scanGeneration) {
                await this._stopCameraInstance(this.html5Qrcode);
                return false;
            }

            this._setControlsState("scanning");
            this._setStatus("");
            console.log("QR scanner start finish in js.");
            return true;
            } catch (error) {
            this._setControlsState("stopped");
            this._setStatus("Could not start camera. Use the file option or tap Start scanning.");
            console.error("Error starting QR scanner:", error);
            return false;
        }

        }, 500);
        
    },

    _stopCameraInstance: async function (scanner) {
        if (!scanner) {
            return;
        }
        try {
            if (this._isScanningState(scanner)) {
                await scanner.stop();
            }
        } catch (error) {
            console.error("Error stopping QR scanner:", error);
        }
    },

    scanQR: async function (dotHelper, isScan) {
        var scanGeneration = this._scanGeneration;
        try {
            var qrElement = document.getElementById("qr-reader");
            if (qrElement == null) {
                return false;
            }

            this.ensureScannerControls();

            function onScanSuccess(decodedText, decodedResult) {
                if (!dotHelper || !window.ScanQRCodeJS._scanContext) {
                    return;
                }
                var qrCode = { qrCode: decodedText };
                dotHelper.invokeMethodAsync('UpdateMessageCaller', qrCode);
            }

            function onErrorScan(err) {
                if (!dotHelper || !window.ScanQRCodeJS._scanContext) {
                    return;
                }
                if (!isScan) {
                    const errorMessage = err.toString().substring(0, 55);
                    const cleanedErrString = errorMessage.replace(/\s+/g, "");
                    if (cleanedErrString.includes("NoMultiFormatReaders")) {
                        dotHelper.invokeMethodAsync('UpdateMessageCaller', "fallback-qr-scanner");
                    }
                }
            }

            this._scanContext = {
                dotHelper: dotHelper,
                isScan: isScan,
                onScanSuccess: onScanSuccess,
                onErrorScan: onErrorScan
            };

            if (this.html5Qrcode) {
                await this._clearScannerInstance();
            }

            if (scanGeneration !== this._scanGeneration) {
                return false;
            }

            this.html5Qrcode = new Html5Qrcode("qr-reader");
            return await this._startCamera(scanGeneration);
        } catch (error) {
            if (scanGeneration !== this._scanGeneration) {
                this.stopCameraTracks();
            }
            this._setControlsState("stopped");
            console.error("Error initializing QR scanner:", error);
            return false;
        }
    },

    startScanning: async function () {
        return await this._startCamera(this._scanGeneration);
    },

    stopScanning: async function () {
        if (!this.html5Qrcode) {
            this._setControlsState("stopped");
            return true;
        }

        await this._stopCameraInstance(this.html5Qrcode);
        this._setControlsState("stopped");
        this._setStatus("Scanning stopped.");
        return true;
    },

    BrowseClick: function () {
        var fileInput = document.getElementById("qr-file-input");
        if (fileInput) {
            fileInput.value = "";
            fileInput.click();
        }
    },

    _onFileSelected: async function (event) {
        var file = event.target && event.target.files && event.target.files[0];
        if (!file || !this.html5Qrcode || !this._scanContext) {
            return;
        }

        this._setStatus("Scanning image...");
        try {
            if (this._isScanningState(this.html5Qrcode)) {
                await this._stopCameraInstance(this.html5Qrcode);
                this._setControlsState("stopped");
            }

            var decodedText = await this.html5Qrcode.scanFile(file, true);
            this._scanContext.onScanSuccess(decodedText, { decodedText: decodedText });
            this._setStatus("");
        } catch (error) {
            this._setStatus("No QR code found in this image.");
            console.error("Error scanning file:", error);
        }
    },

    clearValue: async function () {
        if (this.html5Qrcode && this._isScanningState(this.html5Qrcode)) {
            try {
                await this.html5Qrcode.pause(true);
            } catch (error) {
                console.error("Error pausing QR scanner:", error);
            }
        }
    },

    _clearScannerInstance: async function () {
        var scanner = this.html5Qrcode;
        this.html5Qrcode = undefined;
        this._scanContext = null;
        this._unbindControls();

        if (scanner) {
            try {
                await this._stopCameraInstance(scanner);
                await scanner.clear();
            } catch (error) {
                console.error("Error disposing scanner:", error);
            }
        }

        this._setControlsState("stopped");
        this._setStatus("");
        this.stopCameraTracks();
    },

    disposeScan: async function (scanName) {
        this._scanGeneration += 1;
        await this._clearScannerInstance();
        return true;
    },

    resizescanner: async function () {
        if (!this.html5Qrcode || !this._isScanningState(this.html5Qrcode)) {
            return;
        }

        var scanGeneration = this._scanGeneration;
        await this._stopCameraInstance(this.html5Qrcode);
        if (scanGeneration === this._scanGeneration) {
            await this._startCamera(scanGeneration);
        }
    },

    qrStopScanning: async function () {
        return await this.stopScanning();
    }
};

window.ScanQRCodeJS._handlers.resize = function () {
    window.ScanQRCodeJS.resizescanner();
};

window.addEventListener('resize', window.ScanQRCodeJS._handlers.resize);
