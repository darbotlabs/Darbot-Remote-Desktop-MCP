# Ubuntu RDP and Remote Access Solutions

This guide provides comprehensive information about setting up and using GUI RDP solutions for Ubuntu systems, along with SSH connectivity options available through RetroRDP.

## 🐧 Ubuntu GUI RDP Solutions (Open Source)

### 1. XRDP Server (Recommended)
**Best for**: Traditional Ubuntu desktop access via RDP

```bash
# Install XRDP on Ubuntu server
sudo apt update
sudo apt install -y xrdp

# Enable and start XRDP
sudo systemctl enable xrdp
sudo systemctl start xrdp

# Configure firewall
sudo ufw allow 3389/tcp

# Add user to ssl-cert group (optional)
sudo usermod -a -G ssl-cert $USER
```

**Connection**: Use RetroRDP's standard RDP connection (port 3389) to connect to your Ubuntu machine.

### 2. TigerVNC Server
**Best for**: Lightweight VNC-based remote desktop

```bash
# Install TigerVNC Server
sudo apt update
sudo apt install -y tigervnc-standalone-server tigervnc-common

# Set VNC password
vncpasswd

# Start VNC server
vncserver :1 -geometry 1920x1080 -depth 24

# Install VNC client for Windows
# Download TigerVNC Viewer from: https://tigervnc.org/
```

**Connection**: Use VNC client on Windows to connect (port 5901).

### 3. x11vnc + Desktop Environment
**Best for**: Sharing existing desktop session

```bash
# Install x11vnc
sudo apt update
sudo apt install -y x11vnc

# Create password
x11vnc -storepasswd

# Start x11vnc server
x11vnc -forever -usepw -create -shared
```

### 4. NoMachine (Free for personal use)
**Best for**: High-performance remote desktop

```bash
# Download NoMachine for Ubuntu
wget https://download.nomachine.com/download/7.10/Linux/nomachine_7.10.1_1_amd64.deb
sudo dpkg -i nomachine_7.10.1_1_amd64.deb

# Install Windows client
# Download from: https://www.nomachine.com/download
```

### 5. Chrome Remote Desktop
**Best for**: Browser-based access

```bash
# Download Chrome Remote Desktop
wget -q -O - https://dl.google.com/linux/linux_signing_key.pub | sudo apt-key add -
echo "deb [arch=amd64] http://dl.google.com/linux/chrome-remote-desktop/deb/ stable main" | sudo tee /etc/apt/sources.list.d/chrome-remote-desktop.list
sudo apt update
sudo apt install -y chrome-remote-desktop
```

## 🔒 SSH Connectivity with RetroRDP

RetroRDP now includes comprehensive SSH support for Ubuntu/Linux server management:

### SSH File Transfer (SFTP)
- **Purpose**: Transfer files between Windows and Linux systems
- **Features**: 
  - Dual-pane file browser (Windows ↔ Linux)
  - Drag-and-drop file transfers
  - Directory navigation and management
  - File permissions display
  - Bulk file operations

**Usage**:
1. In RetroRDP, click "🔒 SSH File Transfer" in navigation
2. Enter server details (IP, username, password)
3. Connect and manage files between systems

### SSH Terminal Console
- **Purpose**: Command-line access to Ubuntu/Linux servers
- **Features**:
  - Full terminal emulation
  - Real-time command execution
  - Command history
  - Secure encrypted connection

**Usage**:
1. In RetroRDP, click "🐧 SSH Terminal" in navigation
2. Enter server connection details
3. Execute commands directly on remote server

### Connection Setup Examples

#### Connect to Ubuntu Server via SSH
```yaml
Connection Type: SSH Terminal Console
Server Address: 192.168.1.100
Username: ubuntu
Port: 22
```

#### File Transfer to Linux Server
```yaml
Connection Type: SSH File Transfer (SFTP)
Server Address: 192.168.1.100
Username: root
Port: 22
```

## 🛡️ Security Configuration

### Ubuntu Server SSH Setup
```bash
# Install OpenSSH server
sudo apt update
sudo apt install -y openssh-server

# Configure SSH (edit /etc/ssh/sshd_config)
sudo nano /etc/ssh/sshd_config

# Recommended settings:
Port 22
PermitRootLogin yes  # or 'no' for security
PasswordAuthentication yes
PubkeyAuthentication yes

# Restart SSH service
sudo systemctl restart ssh
sudo systemctl enable ssh

# Configure firewall
sudo ufw allow ssh
sudo ufw enable
```

### Key-Based Authentication (Recommended)
```bash
# Generate SSH key pair on Windows (in PowerShell)
ssh-keygen -t rsa -b 4096

# Copy public key to Ubuntu server
scp ~/.ssh/id_rsa.pub user@server:~/.ssh/authorized_keys

# Set proper permissions on Ubuntu
chmod 700 ~/.ssh
chmod 600 ~/.ssh/authorized_keys
```

## 🖥️ Desktop Environment Options

### For XRDP Compatibility

#### XFCE (Lightweight)
```bash
sudo apt install -y xfce4 xfce4-goodies
echo "xfce4-session" > ~/.xsession
```

#### GNOME (Feature-rich)
```bash
sudo apt install -y ubuntu-desktop-minimal
# Or full desktop: sudo apt install -y ubuntu-desktop
```

#### MATE (Traditional)
```bash
sudo apt install -y ubuntu-mate-desktop
```

## 🔧 Troubleshooting

### Common XRDP Issues

**Black screen after login**:
```bash
# Solution 1: Use XFCE
sudo apt install -y xfce4
echo "xfce4-session" > ~/.xsession

# Solution 2: Configure GNOME for XRDP
sudo apt install -y gnome-session-flashback
```

**Connection refused**:
```bash
# Check XRDP status
sudo systemctl status xrdp

# Check firewall
sudo ufw status
sudo ufw allow 3389/tcp

# Check logs
sudo tail -f /var/log/xrdp.log
```

### SSH Connection Issues

**Permission denied**:
```bash
# Check SSH service
sudo systemctl status ssh

# Verify user permissions
sudo usermod -aG sudo username

# Check SSH config
sudo nano /etc/ssh/sshd_config
```

**Connection timeout**:
```bash
# Check firewall
sudo ufw status
sudo ufw allow 22/tcp

# Verify SSH is listening
sudo netstat -tlnp | grep :22
```

## 📱 Mobile Access Options

### For SSH
- **JuiceSSH** (Android)
- **Termius** (iOS/Android)
- **PuTTY Mobile** (Windows Mobile)

### For RDP
- **Microsoft Remote Desktop** (Official app)
- **RD Client** (iOS/Android)

## 🌟 Best Practices

1. **Security First**
   - Use key-based authentication when possible
   - Change default SSH port if needed
   - Enable firewall with minimal required ports
   - Regular security updates

2. **Performance**
   - Use lightweight desktop environments for RDP
   - Optimize screen resolution for network bandwidth
   - Consider compression settings

3. **Backup**
   - Regular system backups
   - SSH key backups
   - Configuration backups

## 🔗 Integration with RetroRDP

RetroRDP seamlessly integrates multiple connection types:

- **RDP Sessions**: Traditional Windows Remote Desktop to Ubuntu XRDP
- **SSH File Transfer**: Secure file management with SFTP
- **SSH Terminal**: Command-line server administration
- **Unified Interface**: All connection types in one application

This makes RetroRDP a comprehensive solution for managing both Windows and Linux systems from a single, retro-themed interface.

---

*For additional support and advanced configurations, refer to the main [User Guide](../UserGuide.md) and [Setup Guide](Setup-Guide.md).*