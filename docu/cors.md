## CORS
To use `CORS` just add `CORS` settings in your `appsettings.*.json` (or dependent on your configuration providers environment variabley, appconfiguration services,secets.json..)

```json
{
  "CorsSettings": {
    "Origins": "*",
    "Headers": "Origin, X-Requested-With, Content-Type, Accept",
    "AllowCredentials": true
  }
}
```

