# Protocole Roomers
## Qui parle à qui

Le client Rust envoie des messages au serveur .NET. Le serveur applique les règles de la salle, puis prévient le dashboard Angular à chaque changement.

## Ce que le client peut dire au serveur

Un message par ligne, en JSON, en TCP.

- Un étudiant entre.
- Un étudiant sort.
- Cet étudiant est toujours là (signe de vie régulier).
- Le professeur entre.
- Le professeur sort.
- Le professeur lance le cours pour tant de secondes.

Chaque message dit aussi dans quelle salle ça se passe et à quelle heure. Le serveur ne répond pas au client : il décide tout seul.

## Les règles de la salle

- Tant que le cours n'est pas lancé, la salle est ouverte : on peut entrer et sortir.
- Le professeur doit être dans la salle pour lancer le cours.
- Une fois le cours lancé, plus personne ne peut entrer. Les étudiants peuvent encore partir.
- Quand le temps est écoulé, tous les étudiants sortent automatiquement. Le professeur reste et peut relancer un cours.

## Ce que le serveur dit au dashboard

À chaque changement :

- la salle est-elle ouverte ou en cours,
- quand le cours se termine,
- quel professeur est là,
- quels étudiants sont présents, et quand on les a vus pour la dernière fois.

## Si le message est mauvais

JSON cassé, champ manquant, type inconnu, entrée pendant un cours : le serveur l'ignore, note l'erreur dans ses logs, et continue sans couper la connexion.