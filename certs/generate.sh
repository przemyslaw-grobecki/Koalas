#!/bin/bash
# Generate self-signed CA and service certificates for inter-service TLS

set -e

CERTS_DIR="$(cd "$(dirname "$0")" && pwd)"
PFX_PASSWORD="zoo1234"

# Clean old certs
rm -f "$CERTS_DIR"/*.crt "$CERTS_DIR"/*.key "$CERTS_DIR"/*.pfx "$CERTS_DIR"/*.srl

# Generate CA private key and self-signed certificate
openssl genrsa -out "$CERTS_DIR/ca.key" 2048
openssl req -new -x509 -days 365 -key "$CERTS_DIR/ca.key" \
  -out "$CERTS_DIR/ca.crt" \
  -subj "/C=PL/O=ZooManagement/CN=ZooManagement-CA"

generate_cert() {
  local service_name=$1
  local san=$2

  # Generate private key
  openssl genrsa -out "$CERTS_DIR/$service_name.key" 2048

  # Generate CSR with SAN
  cat > "$CERTS_DIR/$service_name.cnf" <<EOF
[req]
default_bits = 2048
prompt = no
distinguished_name = dn
req_extensions = v3_req

[dn]
C = PL
O = ZooManagement
CN = $service_name

[v3_req]
subjectAltName = $san
EOF

  openssl req -new -key "$CERTS_DIR/$service_name.key" \
    -out "$CERTS_DIR/$service_name.csr" \
    -config "$CERTS_DIR/$service_name.cnf"

  # Sign with CA
  cat > "$CERTS_DIR/$service_name_ext.cnf" <<EOF
[v3_req]
subjectAltName = $san
basicConstraints = CA:FALSE
keyUsage = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth, clientAuth
EOF

  openssl x509 -req -in "$CERTS_DIR/$service_name.csr" \
    -CA "$CERTS_DIR/ca.crt" \
    -CAkey "$CERTS_DIR/ca.key" \
    -CAcreateserial \
    -out "$CERTS_DIR/$service_name.crt" \
    -days 365 \
    -extfile "$CERTS_DIR/$service_name_ext.cnf"

  # Create PFX for Kestrel (ASP.NET)
  openssl pkcs12 -export \
    -in "$CERTS_DIR/$service_name.crt" \
    -inkey "$CERTS_DIR/$service_name.key" \
    -certfile "$CERTS_DIR/ca.crt" \
    -out "$CERTS_DIR/$service_name.pfx" \
    -passout pass:$PFX_PASSWORD

  # Cleanup temp files
  rm -f "$CERTS_DIR/$service_name.csr" "$CERTS_DIR/$service_name.cnf" "$CERTS_DIR/$service_name_ext.cnf"

  echo "  $service_name: .crt + .key + .pfx"
}

echo "Generating certificates..."
echo ""

# Service certificates with SANs
generate_cert "bambooservice" "DNS:localhost,DNS:bambooservice,DNS:host.docker.internal"
generate_cert "koalaservice" "DNS:localhost,DNS:koalaservice,DNS:host.docker.internal"
generate_cert "postgres" "DNS:localhost,DNS:postgres,DNS:host.docker.internal"

# Cleanup CA temp files
rm -f "$CERTS_DIR/ca.srl"

echo ""
echo "Certificates generated:"
echo "  CA:              ca.crt / ca.key"
echo "  PFX password:    $PFX_PASSWORD"
echo ""
ls -la "$CERTS_DIR"/*.crt "$CERTS_DIR"/*.key "$CERTS_DIR"/*.pfx