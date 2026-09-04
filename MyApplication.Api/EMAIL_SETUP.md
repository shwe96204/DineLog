# Email Setup

The API sends verification and password reset emails through the `Email` section in `appsettings.json` or `appsettings.Development.json`.

## Quick Gmail SMTP setup

1. Turn on 2-Step Verification for your Google account.
2. Create a Google App Password.
3. Fill the `Email` section like this:

```json
"Email": {
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "youraddress@gmail.com",
  "Password": "your-google-app-password",
  "FromAddress": "youraddress@gmail.com",
  "FromName": "MyApplication",
  "EnableSsl": true,
  "UseDevelopmentLoggingFallback": true
}
```

## Development behavior

If `Username`, `Password`, or `FromAddress` are missing and the API is running in development, the app does not send a real email. Instead, it logs the message body to the API console output.

## Recommended safer setup

Avoid committing real credentials into source control. Prefer environment variables or user secrets:

- `Email__Host`
- `Email__Port`
- `Email__Username`
- `Email__Password`
- `Email__FromAddress`
- `Email__FromName`
- `Email__EnableSsl`
- `Email__UseDevelopmentLoggingFallback`
