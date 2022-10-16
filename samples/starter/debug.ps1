.\build.ps1

if ($?) {
    iotedgehubdev start -d ./deployment.debug.json -v
}
