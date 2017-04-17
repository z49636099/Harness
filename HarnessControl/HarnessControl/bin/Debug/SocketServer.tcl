#source D:\\TCLTool\\SocketServer.tcl

	#Create server ; Port = Server port
	proc CreateSocketServer {Port} {
		return [socket -server accept $Port]
	}
	#when client is connected , trigger this event ; 
	#chan = socket channel ; addr = client address ; port = client port
	proc accept {chan addr port} {
		puts "Client Address : $addr ;Client  Port : $port is Connect"
		fconfigure $chan -blocking 0 -buffering line
		fileevent $chan readable [list readsock $chan $addr $port] 
	}
	
	#when server receive data , trigger this event;
	#sock = socket channel ; addr = client address ; port = client port
	proc readsock {sock addr port} {
		if {![eof $sock] } {
			set line [gets $sock]
			puts "Client Address : $addr Port : $port say : $line"
			if {[catch {set echo [Echo $line]} errorValue] == 0} {
				catch {puts $sock $echo}
			} else	{
				puts $sock "Error : $errorValue "
				puts $errorValue
			}
			flush $sock
		} else {
			puts "Client Address : $addr Port : $port is Disconnect"
			close $sock
		}
	}
	
	proc Echo {line} {
		if {[string match "get *" $line]} {
			set tmp [string trimleft $line "get "]
			set Return "[expr $tmp]"
		} else {
			set Return "[eval $line]"
		}
			return $Return
	}

#CreateSocketServer 4660
