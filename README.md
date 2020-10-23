# NWS_Social_Network API
 Projet éducatif dont le but est de développer une application web faisant office de réseau social pour l'école.
 
 Base de données MySQL localhost, les données de connexion sont établies dans le fichier appsettings.json si la configuration doit être changée
 
 Run "dotnet ef database update <Migration>" pour mettre à jour la base de données

## Lancer le backend
Il faut aller dans le repertoire ou le code est présent (NWSSocial), veiller à ce qu'aucune base mysql ne soit lancer (via wamp ou autres) et lancer cette commande :
```bash
docker-compose up -d
```
L'application est maintenant lancée et accessible via le port 8081

## Mettre à jour la base de données avec docker
Récupérez l'id du container via la commande suivante
```bash
docker ps
```

Lancez ensuite cette commande pour lancer les migrations
```bash
docker exec -it "ID DU CONTAINER" dotnet ef database update <Migration> 
```
