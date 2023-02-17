ARG GAME_CI_UNITY_EDITOR_IMAGE

FROM $GAME_CI_UNITY_EDITOR_IMAGE

RUN powershell -Command $Env:JAVA_HOME="C:\Program Files\Unity\Hub\Editor\%version%\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK"
RUN powershell -Command [System.Environment]::SetEnvironmentVariable("Path", [System.Environment]::GetEnvironmentVariable('Path', [System.EnvironmentVariableTarget]::Machine) + ";$($env:JAVA_HOME)\bin")
RUN powershell -Command "Y" | & "C:\Program Files\Unity\Hub\Editor\%version%\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\tools\bin\sdkmanager.bat" "platforms;android-28"
