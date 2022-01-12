#!/bin/bash
rm -rf device*
rm -rf verify*

# Step 1: generate private key "deviceCA-private.key" (root private key)
echo "==========> Step 1: Generate root private key (deviceCA-private.key)"
openssl genrsa -passout pass:1111 -des3  -out deviceCA-private.key 2048
# password: 1234 (important for CA-signed certificates of clients)

# Step 2: generate a certificate with private key (root CA certificate)
echo "==========> Step 2: Generate root CA certificate with previous private key (deviceCA.pem )"
openssl req -x509 -new -nodes -passin pass:1111 -key deviceCA-private.key -sha256 -days 3650 -out deviceCA.pem 

# step 3: create CA-signed certificates for my Azure IoT devices
echo "==========> Step 3: Create device private key (device-private.key)"
openssl genrsa -out device-private.key 2048

echo "==========> Step 4: Create Certificate Signing Request (device.csr) with deviceID (e.g: wgm160p)"
openssl req -new -key device-private.key -out device.csr

echo "==========> Step 5: Create device CA-signed certificate with using pass 1111 (deviceCert.pem)"
openssl x509 -req -in device.csr -passin pass:1111 -CA deviceCA.pem -CAkey deviceCA-private.key \
            -CAcreateserial -days 3650 -sha256 -out deviceCert.crt 
echo "--> Convert deviceCert.crt to deviceCert.pem"
openssl x509 -in deviceCert.crt -out deviceCert.pem -outform PEM

# step 6: Proof of possesion
echo "==========> Step 6: After adding device to IoT Hub, create proof key (verifyCA.key)"
openssl genrsa -out verifyCA.key 2048

echo "==========> Step 7: Create proof CSR with verification code (verifyCA.csr) with common name is Verification Code from Azure"
openssl req -new -key verifyCA.key -out verifyCA.csr

echo "==========> Step 8: Create proof certificate (verifyCA.pem)"
openssl x509 -req -in verifyCA.csr -passin pass:1111 -CA deviceCA.pem -CAkey deviceCA-private.key -CAcreateserial -days 3650 -sha256 -out verifyCA.pem 

echo "==> Creating final_certs folder --> Go to final_certs to get all important certificates"
rm -rf final_certs
mkdir final_certs
cp BaltimoreCyberTrustRoot.crt.pem ./final_certs
cp deviceCert.crt ./final_certs
cp device-private.key ./final_certs