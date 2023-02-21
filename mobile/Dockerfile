ARG GAME_CI_UNITY_EDITOR_IMAGE
FROM $GAME_CI_UNITY_EDITOR_IMAGE

SHELL ["cmd", "/S", "/C"]
ARG UNITY_VERSION

RUN setx path "C:\\Program Files\\Git\\bin;%path%"
WORKDIR "c:\\Program Files\\Unity\\Hub\\Editor\\${UNITY_VERSION}\\Editor\\Data\\PlaybackEngines\\AndroidPlayer\\SDK\\tools\\bin\\"
ENV JAVA_HOME="C:\\Program Files\\Unity\\Hub\\Editor\\${UNITY_VERSION}\\Editor\\Data\\PlaybackEngines\\AndroidPlayer\\OpenJDK"

SHELL ["bash.exe", "-c"]

RUN "yes | ./sdkmanager.bat --install 'build-tools;28.0.0'"
RUN "yes | ./sdkmanager.bat --install 'platforms;android-28'"
