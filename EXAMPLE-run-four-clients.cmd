:: SERVER
:: if you would like the server to resume computation using some specified file from `n` state-computation then please launch the following:
::   start run-server n 20 http://localhost:62752 path/to/file/all-history-n-20-123.xml
:: parameter explanation:
:: 8 stands for automaton state count
:: 20 stands for the number of collected
start run-server 7 20 http://localhost:62752

:: CLIENTS
start run-client http://localhost:62752
start run-client http://localhost:62752
start run-client http://localhost:62752
start run-client http://localhost:62752

:: PRESENTATION
start run-presentation http://localhost:62752