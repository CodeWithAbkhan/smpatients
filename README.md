```markdown
# Project Introduction

This project is a patients management system developed with ASP.NET Core Blazor .NET 8, featuring full-stack development. It is deployed on a Virtual Private Server (VPS) running Ubuntu 22.04 using Docker. The database is powered by SQL Server deployed within a Docker container.

![Project Image](/smpatients/wwwroot/images/sm-patients-management-system-project-build-with-aspnet-core-dot-net-8-full-stack-docker-sql-server-image-deployed-on-vps-ubuntu-using-docker.png)

## Run Project

To run the project, execute the following commands:
```bash
dotnet ef migrations add initialCreate -o Data/Migrations
dotnet ef database update
dotnet run
```
![Display Patient Record Details](/wwwroot/images/sm-patients-management-system-project-display-patient-record-details-build-with-aspnet-core-dot-net-8-full-stack-docker-sql-server-image-deployed-on-vps-ubuntu-using-docker.png)

![Create New Patient](/smpatients/wwwroot/images/sm-patients-management-system-project-create-new-patient-build-with-aspnet-core-dot-net-8-full-stack-docker-sql-server-image-deployed-on-vps-ubuntu-using-docker.png)

![Upload files and images](/smpatients/wwwroot/images/sm-patients-management-system-project-display-patient-details-build-with-aspnet-core-dot-net-8-full-stack-docker-sql-server-image-deployed-on-vps-ubuntu-using-docker.png)

## Features

- **Patient CRUD Feature**
- **Patient Images**
- **Patient Reviews**
- **Patient File Upload CRUD Feature**
- **Patient Videos CRUD Feature**
- **Assign Patient Records to Roles**

### API Data Fetching

- PDF Viewer using iframe
- Text File Viewer
- Videos Viewer
- Profile Settings for User

### User Roles CRUD by Admin

- Manage Users by Admin
- Manage Roles by Admin
- Assign Roles to Users and Delete Users from Role
- Display All Users in a Role

### Data Grid using FluentUI.net

- Data Grid to Display Patients with Pagination (Accessible based on User Role)
- Data Grid with Search, Categories, and Sorting
- Connected Data Grid with Landbot API to Fetch Fresh Patient Data
- Note: Dashboard functionality is still in progress

### Patient Display Card Information

- Full Bio of Patient
- Dynamic Image Slider
- Patient Reviews Content with Date and Creator
- Dynamic Reviews for Each Patient
- Display Files (Name, Date, Creator); Files can be opened, downloaded, and deleted by admin
- PDF Count Method to Determine PDF Pages
- Convert PDF to Images and Display in Image Slider
- Videos Playable, Downloadable, and Deletable

### Blazor Dialog (FluentUI) for Non-Admin Users

- Upload Files/Images/Videos for Patient
- Add Reviews for Patient (Also Upload Files/Images/Videos for Each Review)

## VPS Ubuntu Deployment

- Push to GitHub Repo

## SQL Server Container Setup

To set up SQL Server container, execute the following command:
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Password@123" -e "MSSQL_PID=Evaluation" -p 1433:1433 --name sqlpreview --hostname sqlpreview -d --rm mcr.microsoft.com/mssql/server:2022-preview-ubuntu-22.04
```
Remember to change connection string with local DB connection string.

## Docker Login and Features

```bash
docker login -u root -p Passwor-vps-here http://sqlpey.com:443
echo "Passwor-vps-here" | docker login -u root --password-stdin http://sqlpey.com:443
```

## Persist Data Volume

```bash
mkdir -p /smpatients/root/sqlpreview_data
chown -R $(whoami) /smpatients/root/sqlpreview_data
chmod -R 777 /smpatients/root/sqlpreview_data
docker inspect -f '{{ .Mounts }}' sqlpreview
docker start sqlpreview
```

## Docker Daemon Configuration

```bash
sudo apt-get update
sudo nano /etc/docker/daemon.json
```
Add the following configuration:

```json
{
  "hosts": ["unix:///var/run/docker.sock"],
  "proxies": {
    "http-proxy": "http://sqlpey.com:80",
    "https-proxy": "https://sqlpey.com:443"
  }
}
```
Then restart Docker:

```bash
sudo systemctl restart docker
sudo ufw allow 443=
Passwor-vps-here
sudo apt-get install docker-ce docker-ce-cli containerd.io
sudo dockerd --containerd=/run/containerd/containerd.sock
```

## Software and Application Setup

```bash
sudo apt update
sudo apt upgrade -y
sudo apt install docker.io -y
sudo systemctl start docker
sudo systemctl enable docker
export CR_PAT= github key secrete here
echo $CR_PAT | docker login ghcr.io -u CodeWithAbkhan --password-stdin
docker pull ghcr.io/codewithabkhan/smpatients:1.0.0
```

## Docker Compose Configuration (docker-compose.yml)

Create the `docker-compose.yml` file with the following contents:

```yaml
version: '3.3'
services:
  frontend:
    container_name: frontend
    image: ghcr.io/codewithabkhan/smpatients:1.0.0
    command: /bin/bash
    user: root
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    volumes:
      - /smpatients/frontend/files:/app/wwwroot/files
      - /smpatients/frontend/images:/app/wwwroot/images
      - /smpatients/frontend/videos:/app/wwwroot/videos
      - /smpatients/frontend/documents:/app/wwwroot/documents
      - /smpatients/frontend/others:/app/wwwroot/others
```

Execute the command `docker compose up -d` to start the application.

## Web Application Permissions and Configuration

```bash
mkdir /smpatients/frontend/files
mkdir /smpatients/frontend/images
mkdir /smpatients/frontend/documents
mkdir /smpatients/frontend/videos
mkdir /smpatients/frontend/others
```

Adjust permissions:

```bash
sudo chown -R root:root /smpatients/frontend
chmod -R 755 /smpatients/frontend
chmod -R 755 /smpatients/frontend/files
```

Verify permissions and execute additional commands as needed.

```

This markdown provides a comprehensive guide for setting up and running the SM Patients Management System Project. Ensure to follow the steps carefully for successful deployment and utilization of the system.
```