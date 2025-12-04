-- Create databases for each microservice
CREATE DATABASE productdb;
CREATE DATABASE orderdb;
CREATE DATABASE inventorydb;

-- Grant permissions (optional, but good practice)
GRANT ALL PRIVILEGES ON DATABASE productdb TO postgres;
GRANT ALL PRIVILEGES ON DATABASE orderdb TO postgres;
GRANT ALL PRIVILEGES ON DATABASE inventorydb TO postgres;
