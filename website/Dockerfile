FROM nginx:alpine

# Copier le site web dans le répertoire par défaut de Nginx
COPY index.html /usr/share/nginx/html/
COPY WindRisesWindows.zip /usr/share/nginx/html/

# Copier la configuration Nginx personnalisée pour le port 5555
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Exposer le port 5555
EXPOSE 5555

# Démarrer Nginx
CMD ["nginx", "-g", "daemon off;"]
