# CentCom.Exporter

Use this utility to export your bans over HTTP to a standardized format which can be consumed by ``CentCom.Server``
instances.

## Functionality

This application exposes bans from a configured database on an HTTP REST API, available at ``api/ban`` from the root
url. By default, without any query parameters, this URL will provide the latest collection of bans up to the configured
limit. By providing a ``cursor`` parameter of a ban ID, the bans will be the collection of bans up to the limit starting
at the cursor ID. Note: the **cursor ID is not inclusive**, so as to say the bans will start at the ID after the cursor.
This is very helpful for any programmatic parsing of the bans, as you will never receive a duplicate of a ban you
already processed.

## Supported Codebases

This application currently supports the following codebases, and their downstreams assuming no extreme modification to
ban database formats:

- [/tg/station](Data/Providers/TgBanProvider.cs)
- [ParadiseSS13](Data/Providers/ParadiseBanProvider.cs)

Don't see a codebase that applies to you? Feel free to provide your own implementation
of [IBanProvider](Data/Providers/IBanProvider.cs) in a PR, or request that bobbahbrown#0001 provide one.

## Use

[Requires ASP.NET Core Runtime 9+.](https://dotnet.microsoft.com/download/dotnet/9.0)

[Find the latest compiled executables for CentCom.Exporter here](https://github.com/bobbahbrown/CentCom/releases/latest)

To use the exporter, simply configure ``appsettings.json`` as described in the [configuration section](#configuration)
of this README, specifically the provider and connection string. Optionally configure ``hostsettings.json`` to control
the port on which the application will bind to.

To implement HTTPS I personally recommend proxying the service through CloudFlare, or behind NGINX. If you wish to use
HTTPS directly, edit ``hostsettings.json`` to use ``https`` and configure the other settings for Kestrel as appropriate.

## Configuration

Configure your exporter instance through ``appsettings.json``. The possible configuration values are described below,
along with their function. To see a complete example, see [Sample Configuration](#sample-configuration).

### ConnectionStrings.provider

The connection string used to connect to the ban source, specific to your [CentCom.Provider](#centcomprovider) value.

### CentCom.Provider

Controls the ``IBanProvider`` implementation used for your exporter, must be a valid ``BanProviderKind``.
See [BanProviderKind](Configuration/BanProviderKind.cs) for more information

If you are a /tg/ downstream and have not modified your ban format, you should be able to use the ``TgBanProvider`` by
setting this value to ``Tgstation``.

### CentCom.JobBans

Controls what kind of job bans, if any, are provided by the API.
See [BanInclusionOption](Configuration/BanInclusionOption.cs) for possible values.

### CentCom.ServerBans

Controls what kind of server bans, if any, are provided by the API.
See [BanInclusionOption](Configuration/BanInclusionOption.cs) for possible values.

### CentCom.AfterDate

If provided, will only provide bans that are placed after this date.

### CentCom.AfterId

If provided, will only provide bans that are placed after this ID.

### CentCom.Limit

The limit of bans provided per request to the API.

### CentCom.UseLocalTimezone

Boolean operator controlling if the ban provider should *assume* the timestamps from the data source are UTC, or a local
timezone.

### CentCom.UtcOffset

A TimeSpan value dictating a UTC offset override which, if not null, is used in combination
with ``CentCom.UseLocalTimezone``. Does nothing if ``CentCom.UseLocalTimezone`` is false.

Note that this assumes the timestamps coming from the database are offset with this UTC offset value, it does NOT
convert the values at all.

Example value: ``-1:00`` in combination with ``CentCom.UseLocalTimezone`` set to true will result in all bans returned
from the API showing an offset of ``-1:00``.

### Sample Configuration

The following configuration will use the ``Tgstation`` provider, and will list all bans within the database in pages of
50 bans.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "provider": "Server=127.0.0.1;Database=tgstation;Port=3306;Uid=root;Pwd=your-password-here;"
  },
  "CentCom": {
    "Provider": "Tgstation",
    "JobBans": "All",
    "ServerBans": "All",
    "AfterDate": null,
    "AfterId": null,
    "Limit": 50,
    "UseLocalTimezone": false,
    "UtcOffset": null
  },
  "AllowedHosts": "*"
}
```