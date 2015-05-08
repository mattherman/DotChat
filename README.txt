
===============================
= DotChat: A .NET IRC Client  =
===============================


== How To Use ==

1.  Run the application using the ChatClient console application project.
2.  Enter in server information
    a. server => "irc.freenode.net""
    b. port => "6667"
3.  Enter in registration information (all fields require input, cannot take empty string)
    a. nickname => "dotchatclient"
    b. user => "none"
    c. real name => "none"
    d. password => "none"
4.  The client will connect to the server at this point. Wait until connection and registration
    is complete. You will know it is complete when you receive a bunch of notices from the server.
5.  Join a channel. I would suggest choosing one that is empty for testing, but you can join an existing
    channel like #node.js if you would like. All channel names are required to start with "#". Joining a channel
	that doesn't exist will create that channel. I usually use "#myclienttest".
    a. "/join #myclienttest"
6.  You can now send and receive messages simply by typing your message and hitting enter.
    a. To see your messages being sent/received, either open another client and connect to the same channel
       with a different nickname, or use the freenode webchat client at http://webchat.freenode.net/ and
	   connect to the same channel with a different nickname.
7.  You can also send and receive private messages.
    a. "/msg anotheruser Hello, how are you?"
8.  Try translating incoming messages using the "lang" command.
    a. "/lang French" will cause all incoming messages to be translated to French.
    b. "/lang off" will end the translation.
9.  You can use the part command to leave a channel, or just join a different one, which automatically leaves
    the previous one.
    a. "/part"
10. Sick of your nickname? You can change nicknames with the "nick" command.
	a. "/nick mynewnickname"
11. All available commands are listed below, but you can also access help in the application with "/help".
12. When you are finished, quit.
    a. "/quit"


== Supported User Commands ==

/JOIN <channel>
/PART
/MSG <user> <message>
/NICK <nickname>
/HELP
/QUIT

== Sources ==

Numeric Codes
	https://www.alien.net.au/irc/irc2numerics.html
RFC 1459
	https://tools.ietf.org/html/rfc1459
IRC Command List
	http://en.wikipedia.org/wiki/List_of_Internet_Relay_Chat_commands
IRC Basic User Command List
	http://www.ircbeginner.com/ircinfo/ircc-commands.html
Message Parsing
	http://calebdelnay.com/blog/2010/11/parsing-the-irc-message-format-as-a-client