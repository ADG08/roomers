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

## Format d'un message

Tous les messages ont les mêmes champs de base :

| Champ  | Sens                                                        |
|--------|-------------------------------------------------------------|
| `type` | ce qui s'est passé (voir plus bas)                          |
| `room` | le nom de la salle                                          |
| `ts`   | l'heure de l'événement, en ISO 8601 UTC (`2026-10-27T13:30:00Z`) |

Les identifiants (`student_id`, `teacher_id`) sont des `Guid` écrits en texte, par exemple `3f2504e0-4f89-11d3-9a0c-0305e82c3301`. Un identifiant vide (`00000000-0000-0000-0000-000000000000`) est refusé.

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
- Quand le temps est écoulé, tous les étudiants sortent automatiquement. Le professeur reste et peut relancer un cours.

## Ce que le serveur dit au dashboard

À chaque changement :

- la salle est-elle ouverte ou en cours,
- quand le cours se termine,
- quel professeur est là,
- quels étudiants sont présents, et quand on les a vus pour la dernière fois.

## Si le message est mauvais

JSON cassé, champ manquant, type inconnu, identifiant invalide, entrée pendant un cours : le serveur l'ignore, note l'erreur dans ses logs, et continue sans couper la connexion.

## À voir

Des choix pas encore faits, je les tranche en avançant.

- **Fin du cours.** Le domaine sait terminer un cours (`EndCourse`), mais c'est le serveur qui aura un minuteur et qui l'appellera quand la durée est écoulée. À confirmer.
- **Arrêt anticipé.** Le professeur peut-il finir son cours avant la fin ? Si oui, un message `class_ended`.
- **Le professeur part pendant le cours.** Le cours continue-t-il ? S'arrête-t-il ? Aujourd'hui la règle ne dit rien.
- **Un `student_active` d'un étudiant inconnu.** Ignoré, ou considéré comme une entrée ?
- **Ce que voit le client Rust à la fin d'un cours.** Les étudiants sont sortis côté serveur : le client doit-il en être prévenu, vu que le serveur ne répond pas ?
- **Créer une salle.** Qui choisit son nom et sa capacité ? Un fichier de configuration, ou à la première connexion ?
- **Les identifiants.** Qui les génère : le client ou le serveur ?
- **Historique des cours.** Quand la base de données arrivera, que garde-t-on d'un cours terminé (durée, étudiants présents) ?