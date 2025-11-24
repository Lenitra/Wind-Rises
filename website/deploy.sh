#!/bin/bash

echo "========================================"
echo "  Wind Rises - Deploy Docker Container"
echo "========================================"
echo ""

# Couleurs pour les messages
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Variables
IMAGE_FILE="wind-rises-web.tar"
CONTAINER_NAME="wind-rises"
PORT=5555

# Vérifier si Docker est installé
if ! command -v docker &> /dev/null; then
    echo -e "${RED}[ERREUR]${NC} Docker n'est pas installé sur cette machine."
    echo "Installez Docker avant de continuer: https://docs.docker.com/get-docker/"
    exit 1
fi

# Vérifier si Docker est en cours d'exécution
if ! docker info &> /dev/null; then
    echo -e "${RED}[ERREUR]${NC} Docker n'est pas en cours d'exécution."
    echo "Démarrez Docker et réessayez."
    exit 1
fi

echo -e "${GREEN}[INFO]${NC} Docker est actif."
echo ""

# Vérifier si le fichier TAR existe
if [ ! -f "$IMAGE_FILE" ]; then
    echo -e "${RED}[ERREUR]${NC} Le fichier $IMAGE_FILE n'existe pas dans ce dossier."
    echo "Assurez-vous d'avoir copié le fichier TAR ici."
    exit 1
fi

echo -e "${GREEN}[ETAPE 1/5]${NC} Vérification si un conteneur existe déjà..."
if docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo -e "${YELLOW}[INFO]${NC} Le conteneur $CONTAINER_NAME existe déjà."
    echo -e "${YELLOW}[INFO]${NC} Arrêt et suppression du conteneur existant..."
    docker stop "$CONTAINER_NAME" &> /dev/null
    docker rm "$CONTAINER_NAME" &> /dev/null
fi

echo ""
echo -e "${GREEN}[ETAPE 2/5]${NC} Suppression des anciennes images wind-rises-web..."
if docker images --format '{{.Repository}}' | grep -q "^wind-rises-web$"; then
    echo -e "${YELLOW}[INFO]${NC} Suppression de l'ancienne image wind-rises-web..."
    docker rmi wind-rises-web &> /dev/null || true
fi
# Nettoyage des images orphelines (sans tag)
echo -e "${YELLOW}[INFO]${NC} Nettoyage des images orphelines..."
docker image prune -f &> /dev/null

echo ""
echo -e "${GREEN}[ETAPE 3/5]${NC} Chargement de l'image Docker depuis $IMAGE_FILE..."
docker load -i "$IMAGE_FILE"
if [ $? -ne 0 ]; then
    echo -e "${RED}[ERREUR]${NC} Échec du chargement de l'image."
    exit 1
fi

echo ""
echo -e "${GREEN}[ETAPE 4/5]${NC} Vérification si le port $PORT est disponible..."
if lsof -Pi :$PORT -sTCP:LISTEN -t &> /dev/null || netstat -an 2>/dev/null | grep ":$PORT " | grep LISTEN &> /dev/null; then
    echo -e "${YELLOW}[AVERTISSEMENT]${NC} Le port $PORT semble être utilisé."
    read -p "Voulez-vous continuer quand même ? (o/n) " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[OoYy]$ ]]; then
        echo "Déploiement annulé."
        exit 1
    fi
fi

echo ""
echo -e "${GREEN}[ETAPE 5/5]${NC} Démarrage du conteneur..."
docker run -d -p $PORT:$PORT --name "$CONTAINER_NAME" --restart unless-stopped wind-rises-web
if [ $? -ne 0 ]; then
    echo -e "${RED}[ERREUR]${NC} Échec du démarrage du conteneur."
    exit 1
fi

echo ""
echo "========================================"
echo -e "  ${GREEN}DEPLOIEMENT REUSSI !${NC}"
echo "========================================"
echo ""
echo -e "Le site Wind Rises est maintenant accessible à :"
echo -e "  ${GREEN}http://localhost:$PORT${NC}"
echo ""
echo "Commandes utiles :"
echo "  - Voir les logs     : docker logs $CONTAINER_NAME"
echo "  - Arrêter le site   : docker stop $CONTAINER_NAME"
echo "  - Démarrer le site  : docker start $CONTAINER_NAME"
echo "  - Redémarrer        : docker restart $CONTAINER_NAME"
echo "  - Supprimer         : docker rm -f $CONTAINER_NAME"
echo ""
