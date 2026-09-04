window.floatingAnimation = {
    createParticles: function() {
        const container = document.getElementById('particleContainer');
        
        // Add null check to prevent errors
        if (!container) {
            return;
        }
        
        // Remove existing particles to prevent duplicates
        const existingParticles = container.querySelectorAll('.particle');
        existingParticles.forEach(particle => particle.remove());
        
        const particleCount = 45;
        for (let i = 0; i < particleCount; i++) {
            const particle = document.createElement('div');
            particle.classList.add('particle');
            const size = Math.random() * 6 + 2;
            particle.style.width = `${size}px`;
            particle.style.height = `${size}px`;
            particle.style.left = `${Math.random() * 100}%`;
            particle.style.animationDelay = `${Math.random() * 15}s`;
            particle.style.animationDuration = `${8 + Math.random() * 12}s`;

            particle.style.background = `rgba(184, 80, 67, ${Math.random() * 0.4 + 0.1})`;
            particle.style.boxShadow = `0 0 6px rgba(184,80,67,0.6)`;
            container.appendChild(particle);
        }
    }

    

};