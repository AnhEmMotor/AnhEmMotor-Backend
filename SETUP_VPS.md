# Bước 1: Cập nhật thư viện

Chạy câu lệnh sau để cập nhật thư viện. Nếu chưa có cài sudo, buộc phải chạy trong root.

```
apt update && apt upgrade -y
apt install -y sudo curl git unzip libicu-dev build-essential
```

# Bước 2: Tạo người dùng riêng biệt

Ở đây sẽ tạo người dùng github-action. Nếu có sẵn user dùng github action thì bỏ qua bước này

```
sudo adduser -- disabled-password -- gecos "" github-action
usermod -aG sudo github-action
su - github-action
mkdir -p ~/.ssh
chmod 700 ~/.ssh
touch ~/.ssh/authorized_keys
chmod 600 ~/.ssh/authorized_keys
nano ~/.ssh/authorized_keys
```

Sau đó chạy câu lệnh trên máy tính windows, git bash để tạo public key ssh:

```
ssh-keygen -t ed25519 -C "anhemmotor-hosting"
```

Ở bước "Enter file in which to save the key", lưu ý gõ tên khác tên gốc để cho dễ nhớ, sau khi xong thì nó sẽ tạo ra 2 file. 1 file không có pub, 1 file có pub. Paste cái nội dung file .pub cho phần sudo nano trên hosting. Sau đó chạy câu lệnh:

```
sudo visudo /etc/sudoers.d/github-action
```

Thêm cái này vào cuối file. Chú ý, nếu được, sửa câu lệnh này chỉ cho chạy các lệnh cơ bản

```
github-action ALL=(ALL:ALL) NOPASSWD: ALL
```

# Bước 3: Cài đặt PostgreSQL

Chạy câu lệnh sau:

```
sudo apt update
sudo apt install postgresql postgresql-contrib -y
sudo systemctl start postgresql
sudo systemctl enable postgresql
sudo -i -u postgres psql
```

Trong psql gõ như sau:

```
CREATE DATABASE target_project_db;
CREATE USER project_manager_user WITH PASSWORD 'StrongPassword2026';
ALTER DATABASE target_project_db OWNER TO project_manager_user;
\q
```

target_project_db là tên database mong muốn. Nếu viết in hoa, ví dụ AnhEmMotor, khi tạo DB nó chỉ nhận là anhemmotor.
project_manager_user là tên user mong muốn.
Đặt mật khẩu mới nếu bạn thích.

Tiếp theo chạy câu lệnh

```
sudo nano /etc/postgresql/17/main/postgresql.conf
```

Tìm tới những dòng sau và chỉnh lại:

```
tcp_keepalives_idle = 60
tcp_keepalives_interval = 10
tcp_keepalives_count = 5
```

Sau đó chạy lại:

```
sudo systemctl restart postgresql
```

Tiếp theo, chạy câu lệnh sau để check kết nối và lấy chuỗi kết nối (thường theo như cài đặt ở trên thì cứ enter phần host và phần port để giữ nguyên mặc định, trừ khi...bạn tự chỉnh theo ý bạn theo AI nếu muốn tránh cổng này, hoặc chỉnh host, i don't know?)

```
#!/bin/bash
read -p "Enter host [127.0.0.1]: " host
if [ -z "$host" ]; then
    host="127.0.0.1"
fi
read -p "Enter port [5432]: " port
if [ -z "$port" ]; then
    port="5432"
fi
while true; do
    read -p "Enter database (Required): " database
    if [ -n "$database" ]; then
        break
    fi
done
while true; do
    read -p "Enter username (Required): " username
    if [ -n "$username" ]; then
        break
    fi
done
read -p "Enter password: " password
echo ""
if PGPASSWORD="$password" psql -h "$host" -p "$port" -U "$username" -d "$database" -c "SELECT 1;" > /dev/null 2>&1; then
    connectionString="Host=$host;Port=$port;Database=$database;Username=$username;Password=$password;Maximum Pool Size=100;Minimum Pool Size=5;Connection Idle Lifetime=30;Trust Server Certificate=true;"
    echo "RAW CONNECTION STRING:"
    echo "$connectionString"
else
    echo "CONNECTION FAILED! Please check your inputs, credentials or permissions."
    exit 1
fi
```

# Bước 3: Tải dotnet cho người dùng riêng biệt đó

```
curl -L https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh -- version latest -- runtime aspnetcore
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc
source ~/.bashrc
rm dotnet-install.sh
```

# Bước 4: Tạo khoá SSL HTTPS trên Cloudflare (Nếu chưa cài đặt cho tên miền) & Cài đặt Ngnix (Nếu chưa cài đặt cho toàn bộ hosting)

Vào trong Cloudflare Dashboard, vào mục SSL/TLS -> Origin Server, nhấn Create Certificate.

Trong phần tạo mới, chọn:

- Generate private key and CSR with Cloudflare
- List the hostname: Ví dụ như tên miền là atmotor.com, phần này sẽ có 2 cái điền sẵn và giữ nguyên, nhưng buộc phải có: \*.atmotor.com và atmotor.com
- Choose how long before your certificate expires: Chọn thời gian dài nhất, hiện tại lúc viết là 15 năm.

Khi đó, 2 key hiện ra ở phần: Origin Certificate Installation. Với key ở Origin Certificate, bạn chạy câu lệnh sau, paste vào làm nội dung file và save nó:

```
sudo mkdir -p /etc/nginx/ssl
sudo nano /etc/nginx/ssl/anhemmotor.pem
```

Với key ở trong Private Key thì chạy câu lệnh sau và paste vào, và save nó:

```
sudo mkdir -p /etc/nginx/ssl
sudo nano /etc/nginx/ssl/anhemmotor.key
```

Sau đó chạy câu lệnh

```
sudo apt update
sudo apt install nginx -y
```

Tạo file ngnix cho dự án:

```
sudo nano /etc/nginx/sites-available/anhemmotor
```

Nội dung như sau, chỉnh sửa lại địa chỉ chứa dự án, cổng chạy dự án:

```
server {
    listen 80 default_server;
    listen [::]:80 default_server;
    server_name _;
    return 444;
}

server {
    listen 443 ssl default_server;
    listen [::]:443 ssl default_server;
    server_name _;
    ssl_certificate /etc/nginx/ssl/anhemmotor.pem;
    ssl_certificate_key /etc/nginx/ssl/anhemmotor.key;
    return 444;
}

server {
    listen 80;
    server_name anhemmotor.online admin.anhemmotor.online api.anhemmotor.online monitor.anhemmotor.online;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl;
    server_name api.anhemmotor.online;
    ssl_certificate /etc/nginx/ssl/anhemmotor.pem;
    ssl_certificate_key /etc/nginx/ssl/anhemmotor.key;
    client_max_body_size 50M;

    location /metrics {
        allow 127.0.0.1;
        allow ::1;
        deny all;
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Sau đó chạy câu lệnh để link với file vừa mới tạo (nếu đang chỉnh file có sẵn và chạy được thì không cần chạy 2 câu lệnh đầu mà chạy mỗi câu lệnh cuối):

```
sudo ln -s /etc/nginx/sites-available/anhemmotor /etc/nginx/sites-enabled/
sudo rm /etc/nginx/sites-enabled/default
sudo systemctl restart nginx
```

# Bước 5: Tạo service chạy dự án

Chạy câu lệnh sau:

```
sudo nano /etc/systemd/system/anhemmotor.service
```

Dán nội dung sau vào trong nội dung file

```
[Unit]
Description=AnhEmMotor ASP.NET Core Application

[Service]
WorkingDirectory=/var/www/anhemmotor/backend
ExecStart=/home/github-action/.dotnet/dotnet /var/www/anhemmotor/backend/WebAPI.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=anhemmotor
User=github-action
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

Sau đó chạy câu lệnh

```
sudo systemctl daemon-reload
sudo systemctl enable anhemmotor.service
```

# Bước 6: Tạo folder chứa dự án và dữ liệu của dự án

```
sudo chown -R github-action:github-action /var/www/anhemmotor/
sudo chmod -R 755 /var/www/anhemmotor/
sudo mkdir -p /var/www/anhemmotor/backend
sudo mkdir -p /var/www/anhemmotor/data
```
