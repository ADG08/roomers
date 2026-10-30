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
- Le professeur arrête le cours avant la fin.

Chaque message dit aussi dans quelle salle ça se passe et à quelle heure. Le serveur ne répond pas au client : il décide tout seul.

## Format d'un message

Tous les messages ont les mêmes champs de base :

| Champ  | Sens                                                             |
|--------|------------------------------------------------------------------|
| `type` | ce qui s'est passé (voir plus bas)                               |
| `room` | le nom de la salle                                               |
| `ts`   | l'heure de l'événement, en ISO 8601 UTC (`2026-10-27T13:30:00Z`) |

Les identifiants (`student_id`, `teacher_id`) sont des `Guid` écrits en texte, par exemple `3f2504e0-4f89-11d3-9a0c-0305e82c3301`. C'est le client qui les génère. Un identifiant vide (`00000000-0000-0000-0000-000000000000`) est refusé.

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

Signe de vie régulier d'un étudiant déjà présent.

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

Les salles sont déclarées dans la configuration du serveur : un nom et une capacité pour chacune. Un message qui vise une salle inconnue est ignoré.

## Les règles de la salle

### Les places

- Une salle a une capacité d'au moins 10 places.
- Une personne ne peut pas entrer deux fois (même identifiant).
- Il n'y a qu'un seul professeur par salle.
- La dernière place est réservée au professeur : tant qu'il n'est pas là, un étudiant ne peut pas la prendre.
- Une salle pleine refuse tout le monde.

### Le cours

- Tant que le cours n'est pas lancé, la salle est ouverte : on peut entrer et sortir.
- Seul le professeur présent peut lancer le cours.
- Il faut au moins deux personnes dans la salle pour lancer le cours (le professeur et un étudiant).
- On ne peut pas lancer un cours déjà en cours.
- Une fois le cours lancé, plus personne ne peut entrer. Les étudiants peuvent encore partir.
- Si le professeur quitte la salle pendant un cours, le cours s’arrête : les étudiants sortent et la salle se rouvre.
- Le serveur lance un minuteur au début du cours. Quand le temps est écoulé, ou quand le professeur arrête le cours, tous les étudiants sortent automatiquement. Le professeur reste et peut relancer un cours.
- Le serveur ne prévient pas le client de cette fin : les `student_active` envoyés pour un étudiant qui n'est plus dans la salle sont ignorés, et un étudiant peut revenir avec un nouveau `student_joined` une fois la salle rouverte.

## Ce que le serveur dit au dashboard

À chaque changement :

- la salle est-elle ouverte ou en cours,
- quand le cours se termine,
- quel professeur est là,
- quels étudiants sont présents, et quand on les a vus pour la dernière fois.

## Si le message est mauvais

JSON cassé, champ manquant, type inconnu, identifiant invalide, salle inconnue, entrée pendant un cours : le serveur l'ignore, note l'erreur dans ses logs, et continue sans couper la connexion.