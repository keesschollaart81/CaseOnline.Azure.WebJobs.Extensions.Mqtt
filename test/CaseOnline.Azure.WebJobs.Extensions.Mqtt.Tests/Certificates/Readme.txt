Certificates generated using these scripts, the password used is 12345

openssl genrsa -out myRootCA.key 4096
openssl req -x509 -new -nodes -key myRootCA.key -days 3650 -out myRootCA.pem
openssl pkcs12 -export -inkey myRootCA.key -in myRootCA.pem -out myRootCA.pfx 

openssl genrsa -out myTLS.key 2048
openssl req -new -key myTLS.key -out myTLS.req

	# Country Name (2 letter code) [AU]:NL
	# State or Province Name (full name) [Some-State]:ZA
	# Locality Name (eg, city) []:RTD
	# Organization Name (eg, company) [Internet Widgits Pty Ltd]: 
	# Organizational Unit Name (eg, section) []: 
	# Common Name (eg, YOUR name) []:localhost
	# Email Address []: 
	# A challenge password []:12345
	# An optional company name []:
	
openssl x509 -req -in myTLS.req -CA myRootCA.pem -CAkey myRootCA.key -CAcreateserial -out myTLS.pem -days 3650
openssl pkcs12 -export -inkey myTLS.key -in myTLS.pem -out myTLS.pfx 