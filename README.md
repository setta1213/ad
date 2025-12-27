"# ad" 
config {
{
  "Domain": "domain.com",
  "OuPath": "OU=students,DC=domain,DC=com",
  "AdminUser": "username",
  "AdminPassword": "password",
  "IsStarted": false,
  "ApiPort": 5000
}

0.0.0.0/api/main/create,put,get,delete
listapi /api/main/
{(post)/create
[
{
  "studentId": "123456",
  "password": "123456",
  "firstName": "Somchai",
  "lastName": "Jaidee"
}
]
{(post)/delete
[{
  "studentId": "123456",
  }]
}
{(get)/info/%studenid%  เช่น info/12345678}
:5000/api/main/info/%studenid%  เช่น info/12345678}

{(put)/update
[
{
  "studentId": "123456",
  "firstName": "Somchai",
  "lastName": "Jaidee",
  "displayName": "Somchai Jaidee",
  "email": "somchai@testdomain.com",
  "phone": "0812345678",
  "office": "Lab A",
  "profilePath": "\\\\0.0.0.0\\UserHomes\\123456",
  "logonScript": "",
  "homeDirectory": "",
  "homeDrive": "",
  "enabled": true
}

  ]
}

