# Protocole Roomers

## Qui parle à qui

Le client Rust envoie des messages au serveur .NET. Le serveur applique les règles de la salle, puis prévient le dashboard Angular à chaque changement.

## Ce que le client peut dire au serveur

Un message par ligne, en JSON, en TCP (port 9000 par défaut).

- Un étudiant entre.
- Un étudiant sort.
- Cet étudiant est toujours là (signe de vie régulier).
- Le professeur entre.
- Le professeur sort.
- Le professeur lance le cours pour tant de secondes.
- Le professeur arrête le cours avant la fin.

Chaque message dit aussi dans quelle salle ça se passe et à quelle heure. Le serveur ne répond pas au client : il décide tout seul. La connexion reste ouverte tant que le client ne la ferme pas.

## Format d'un message

Tous les messages ont les mêmes champs de base :

| Champ  | Sens                                                                      |
|--------|---------------------------------------------------------------------------|
| `type` | ce qui s'est passé (voir plus bas)                                        |
| `room` | le nom de la salle, jamais vide                                           |
| `ts`   | l'heure de l'événement, en ISO 8601 UTC (`2026-10-27T13:30:00Z`)          |

Pour `ts`, les fractions de seconde sont acceptées (`2026-10-27T13:30:00.123456Z`).

Les identifiants (`student_id`, `teacher_id`) sont des `Guid` écrits en texte, par exemple `3f2504e0-4f89-11d3-9a0c-0305e82c3301`. C'est le client qui les génère. Un identifiant vide (`00000000-0000-0000-0000-000000000000`) est refusé.

Le champ `name` (étudiant ou professeur qui entre) ne peut pas être vide.

## Les messages

### `student_joined`

```json
{"type":"student_joined","room":"Salle 1","ts":"2026-10-27T13:30:00Z","student_id":"3f2504e0-4f89-11d3-9a0c-0305e82c3301","name":"Andre"}
```

### `student_left`

```json
{"type":"student_left","room":"Salle 1","ts":"2026-10-27T13:41:00Z","student_id":"3f2504e0-4f89-11d3-9a0c-0305e82c3301"}
```

### `student_active`

Signe de vie régulier d'un étudiant. Le serveur l'accepte et ne change rien à la salle.

```json
{"type":"student_active","room":"Salle 1","ts":"2026-10-27T13:35:00Z","student_id":"3f2504e0-4f89-11d3-9a0c-0305e82c3301"}
```

### `teacher_joined`

```json
{"type":"teacher_joined","room":"Salle 1","ts":"2026-10-27T13:29:00Z","teacher_id":"9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d","name":"Professeur Morin"}
```

### `teacher_left`

```json
{"type":"teacher_left","room":"Salle 1","ts":"2026-10-27T14:30:00Z","teacher_id":"9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d"}
```

### `class_started`

Le professeur lance le cours. `duration_seconds` doit être supérieur à 0.

```json
{"type":"class_started","room":"Salle 1","ts":"2026-10-27T13:31:00Z","teacher_id":"9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d","duration_seconds":3600}
```

### `class_ended`

Le professeur arrête son cours avant la fin du temps prévu.

```json
{"type":"class_ended","room":"Salle 1","ts":"2026-10-27T13:50:00Z","teacher_id":"9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d"}
```

## Les salles

Les salles sont déclarées dans la configuration du serveur (`appsettings.json`) : un nom et une capacité pour chacune, ainsi que le port d'écoute.

```json
"Tcp": { "Port": 9000 },
"Rooms": [
  { "Name": "Salle 1", "MaxCapacity": 15 },
  { "Name": "Salle 2", "MaxCapacity": 20 }
]
```

Un message qui vise une salle inconnue est refusé.

## Les règles de la salle

### Les places

- Une salle a une capacité d'au moins 10 places.
- Une personne ne peut pas entrer deux fois (même identifiant).
- Il n'y a qu'un seul professeur par salle.
- La dernière place est réservée au professeur : tant qu'il n'est pas là, un étudiant ne peut pas la prendre.
- Une salle pleine refuse tout le monde.
- Seul un message de départ fait sortir une personne : si sa connexion se coupe, elle reste dans la salle.

### Le cours

- Tant que le cours n'est pas lancé, la salle est ouverte : on peut entrer et sortir. Si le professeur part avant le cours, les étudiants restent.
- Seul le professeur présent peut lancer le cours.
- Il faut au moins deux personnes dans la salle pour lancer le cours (le professeur et un étudiant).
- On ne peut pas lancer un cours déjà en cours.
- Une fois le cours lancé, plus personne ne peut entrer. Les étudiants peuvent encore partir.
- Seul le professeur présent peut arrêter le cours avec `class_ended`.
- Si le professeur quitte la salle pendant un cours, le cours s'arrête : les étudiants sortent, le professeur est parti et la salle se rouvre.
- Le serveur lance un minuteur de `duration_seconds` au début du cours. Quand le temps est écoulé, tous les étudiants sortent automatiquement et la salle se rouvre. Le professeur reste et peut relancer un cours. Si le professeur arrête le cours avant la fin, ou s'il part, le minuteur est annulé.
- Le serveur ne prévient pas le client de la fin du cours. Un étudiant peut revenir avec un nouveau `student_joined` une fois la salle rouverte.

## Si le message est mauvais

Deux cas, que le serveur note dans ses logs, sans couper la connexion :

- **Message ignoré** : JSON cassé, champ manquant, type inconnu, identifiant invalide ou vide, nom vide, `duration_seconds` à 0 ou négatif.
- **Événement refusé** : le message est correct mais la salle ne l'accepte pas. Par exemple : salle inconnue, entrée pendant un cours, personne déjà présente, salle pleine, second professeur, cours lancé par un étudiant, départ d'une personne absente.