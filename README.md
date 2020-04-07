# MobileDeliveryMVVM ![MobileDeliveryMVVM Nuget Versions (UMDNuget Artifacts)][logo]
## [v1.4.1](https://dev.azure.com/unitedwindowmfg/United%20Mobile%20Delivery/_packaging?_a=package&feed=UMDNuget&package=MobileDeliveryMVVM&protocolType=NuGet&version=1.4.1&view=versions)

[logo]: https://github.com/vergaraunited/Docs/blob/master/imgs/png/MVVM_icon.png (https://dev.azure.com/unitedwindowmfg/United%20Mobile%20Delivery/_packaging?_a=package&feed=UMDNuget&package=MobileDeliveryMVVM&protocolType=NuGet&version=1.4.1) 

### UMD MVVM - generic/cross platform MVVM sub module

## Configuration
#### Configuration
```xml
<appSettings>
    <add key="LogPath" value="C:\app\logs\" />
    <add key="LogLevel" value="Info" />
    <add key="Url" value="localhost" />
    <add key="Port" value="81" />
    <add key="SQLConn" value="" />
    <add key="WinsysUrl" value="localhost" />
    <add key="WinsysPort" value="8181" />
    <add key="WinsysSrcFilePath" value="\\Fs01\vol1\Winsys32\DATA" />
    <!-- If left empty WinsysDestFilePath defaults to Environment.GetFolderPath(Environment.SpecialFolder.Desktop)-->
    <add key="WinsysDstFilePath" value="" />
    <add key="ClientSettingsProvider.ServiceUri" value="" />
</appSettings>`
```

##### nuget.config file
```xml
<configuration>
  <packageSources>
    <add key="UMDNuget" value="https://pkgs.dev.azure.com/unitedwindowmfg/1e4fcdac-b7c9-4478-823a-109475434848/_packaging/UMDNuget/nuget/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <UMDNuget>
        <add key="Username" value="any" />
        <add key="ClearTextPassword" value="w75dbjeqggfltkt5m65yf3e33fryf2olu22of55jxj4b3nmfkpaa" />
      </UMDNuget>
  </packageSourceCredentials>
</configuration>
```

## NuGet Package References
Package Name            |  Version  |  Description
--------------------    |  -------  |  -----------
MobileDeliverySettings  |   1.4.3   |  Mobile Delivery Settings base code for all configurable components with Symbols
MobileDeliveryClient    |   1.4.0   |  Mobile Delivery Client base code for all clients with Symbols
MobileDeliveryCaching   |   1.4.2   |  Mobile Delivery Cachong base code for all cacheabale clients with Symbols


SubDependencies         |  Versoin  | Thus included in Packages
----------------------  |  -------- |  -------------------------
MobileDeliveryLogger    |   1.3.0   |  Mobile Delivery Logger base code for all components with Symbols
MobileDeliveryGeneral   |   1.4.3   |  Mobile Delivery General Code with Symbols

    
## Configuration
#### Configuration is built into the docker image based on the settings in the app.config

```xml
<appSettings>
    <add key="LogPath" value="C:\app\logs\" />
    <add key="LogLevel" value="Info" />
    <add key="Url" value="localhost" />
    <add key="Port" value="81" />
    <add key="SQLConn" value="" />
    <add key="WinsysUrl" value="localhost" />
    <add key="WinsysPort" value="8181" />
    <add key="WinsysSrcFilePath" value="\\Fs01\vol1\Winsys32\DATA" />
    <!-- If left empty WinsysDestFilePath defaults to Environment.GetFolderPath(Environment.SpecialFolder.Desktop)-->
    <add key="WinsysDstFilePath" value="" />
    <add key="ClientSettingsProvider.ServiceUri" value="" />
</appSettings>`
```

**ToDo**<br/>
**_:x: Built into the docker image based on the settings in the app.config_**<br/>
**_:heavy_exclamation_mark: Consolidate into MobileDeliverySettings_**<br/>
