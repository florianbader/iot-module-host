.\build.ps1

if ($?) {
    iotedgehubdev start -d ./deployment.json -v
}
