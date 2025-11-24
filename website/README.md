# Wind Rises - Site Web

Site vitrine pour le jeu Wind Rises, déployable via Docker.

## 📋 Prérequis

- **Docker Desktop** (Windows/Mac) ou **Docker Engine** (Linux)
- Fichier de build du jeu : `WindRisesWindows.zip`

## 🚀 Déploiement rapide

### Sur Windows

1. **Placer le fichier du jeu**
   ```
   Copiez WindRisesWindows.zip dans le dossier /website
   ```

2. **Build de l'image Docker**
   ```batch
   build.bat
   ```
   Cela va créer le fichier `wind-rises-web.tar`

3. **Test local** (optionnel)
   ```batch
   docker run -d -p 5555:5555 --name wind-rises wind-rises-web
   ```
   Le site sera accessible à http://localhost:5555

### Sur Linux/Mac (Déploiement sur VPS)

1. **Transférer les fichiers sur le serveur**
   ```bash
   scp wind-rises-web.tar deploy.sh user@server:/chemin/destination/
   ```

2. **Se connecter au serveur**
   ```bash
   ssh user@server
   cd /chemin/destination/
   ```

3. **Rendre le script exécutable**
   ```bash
   chmod +x deploy.sh
   ```

4. **Déployer**
   ```bash
   ./deploy.sh
   ```

5. **Ouvrir le port 5555** (si firewall actif)
   ```bash
   # UFW (Ubuntu/Debian)
   sudo ufw allow 5555/tcp

   # Firewalld (CentOS/RHEL)
   sudo firewall-cmd --permanent --add-port=5555/tcp
   sudo firewall-cmd --reload
   ```

## 📁 Structure des fichiers

```
website/
├── index.html              # Page HTML principale
├── WindRisesWindows.zip    # Build du jeu (non versionnée)
├── Dockerfile              # Configuration Docker
├── nginx.conf              # Configuration nginx
├── build.bat               # Script de build (Windows)
├── deploy.sh               # Script de déploiement (Linux/Mac)
├── wind-rises-web.tar      # Image Docker exportée (non versionnée)
└── README.md               # Ce fichier
```

## 🎨 Caractéristiques du site

- **Design responsive** (mobile, tablette, desktop)
- **Thème jour/nuit** (clic sur le soleil/lune)
- **Animations fluides** (nuages, lucioles, particules)
- **Téléchargement direct** du jeu Windows
- **Optimisé** avec GPU acceleration et animations intelligentes

## 🔧 Commandes Docker utiles

```bash
# Voir les logs du conteneur
docker logs wind-rises

# Arrêter le site
docker stop wind-rises

# Démarrer le site
docker start wind-rises

# Redémarrer
docker restart wind-rises

# Supprimer le conteneur
docker rm -f wind-rises

# Voir les conteneurs en cours
docker ps

# Voir toutes les images
docker images
```

## 🔄 Mise à jour du site

### Windows
```batch
# 1. Modifier les fichiers (index.html, nginx.conf, etc.)
# 2. Rebuild l'image
build.bat

# 3. Transférer wind-rises-web.tar sur le serveur
# 4. Sur le serveur, redéployer
./deploy.sh
```

Le script `deploy.sh` va automatiquement :
- Arrêter l'ancien conteneur
- Supprimer l'ancienne image
- Charger la nouvelle image
- Démarrer le nouveau conteneur

## 🌐 Accès au site

- **Local** : http://localhost:5555
- **Production** : http://votre-ip-serveur:5555

## ⚙️ Configuration nginx

Le serveur nginx écoute sur le port **5555** et :
- Sert les fichiers statiques depuis `/usr/share/nginx/html`
- Configure le téléchargement automatique pour les fichiers `.zip`
- Met en cache les fichiers HTML/CSS/JS pendant 1 jour

## 🐛 Dépannage

### Le site ne se charge pas

1. **Vérifier que le conteneur tourne**
   ```bash
   docker ps
   ```

2. **Vérifier les logs**
   ```bash
   docker logs wind-rises
   ```

3. **Vérifier le firewall**
   ```bash
   # Tester localement sur le serveur
   curl http://localhost:5555

   # Si ça marche, c'est un problème de firewall
   sudo ufw status
   ```

4. **Vérifier le port Docker**
   ```bash
   docker port wind-rises
   # Doit afficher: 5555/tcp -> 0.0.0.0:5555
   ```

### Le fichier ZIP ne se télécharge pas

- Vérifiez que `WindRisesWindows.zip` existe dans le dossier `/website` avant le build
- Vérifiez la taille du fichier dans le conteneur :
  ```bash
  docker exec wind-rises ls -lh /usr/share/nginx/html/
  ```

## 📝 Notes

- Les fichiers `*.tar` et `*.zip` sont ignorés par Git (voir `.gitignore`)
- Le conteneur redémarre automatiquement (`--restart unless-stopped`)
- Le site supporte les thèmes clair/sombre avec persistance localStorage

## 🔗 Liens utiles

- [Documentation Docker](https://docs.docker.com/)
- [Documentation nginx](https://nginx.org/en/docs/)
