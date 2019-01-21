*** SERVER
To launch the Server please provide the following parameters:
  run-server.cmd 12 345 http://192.168.145.184:62752 ./file.xml
* where 12 stands for the number of states in the automata
* where 345 stands for the maxium number of collected automata (this limit does not apply to automata violating the Cerny conjecture)
* where http://192.168.145.184:62752 stands for the current address of the workstation (see ipconfig.exe) followed by the port throught which clients will utilise
* where ./file.xml leads to the file where the configuration is to be read from


*** CLIENT
To launch an instance of a Client please provide the following parameters:
  run-client.cmd http://192.168.145.184:62752 12
* where http://192.168.145.184:62752 is the address and port of the Server
* where 12 is the number of threads working concurrently withing the instance

*** PRESENTAION
To launch an instance of a Presentation please provide the following parameters:
  run-presentation.cmd http://192.168.145.184:62752
* where http://192.168.145.184:62752 is the address and port of the Server