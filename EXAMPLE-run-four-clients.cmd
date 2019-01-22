:: SERVER
:: if you would like the server to continue execution using some specified file then please launch the following:
:: start run-server 7 20 http://localhost:62752 path/to/file/all-history-8-20-54.xml
start run-server 7 20 http://localhost:62752

:: CLIENTS
start run-client http://localhost:62752
start run-client http://localhost:62752
start run-client http://localhost:62752
start run-client http://localhost:62752

:: PRESENTATION
start run-presentation http://localhost:62752