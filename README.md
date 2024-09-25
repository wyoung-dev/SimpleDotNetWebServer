# This project demonstrates how to create a simple web server using the .NET implementation of Berkley sockets.

To run the web server, build the project and execute 'ws.exe' (or 'ws') from the build directory. The IP address and port will default to http://127.0.0.1:80 unless specified as arguments, i.e., "ws {ip address} {port}".

After the 'ws' process has started, simply use a curl utility or web browser to connect to the URL http://127.0.0.1/ or http://localhost/ and the response received should be 200 OK with message "Hello, world!"