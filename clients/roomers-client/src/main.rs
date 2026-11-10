use std::io::Write;
use std::net::TcpStream;
use std::thread::sleep;
use std::time::Duration;

use chrono::Utc;
use uuid::Uuid;

mod event;
use event::Event;

fn send(stream: &mut TcpStream, event: &Event) {
    let json = serde_json::to_string(event).expect("serialisation impossible");
    stream.write_all(format!("{}\n", json).as_bytes()).expect("envoi impossible");
}

fn main() {
    let mut stream = TcpStream::connect("127.0.0.1:9000").expect("impossible de se connecter au serveur");
    
    let teacher_id = Uuid::new_v4();
    
    let first_event = Event::TeacherJoined {
        room: "Salle 1".to_string(),
        ts: Utc::now(),
        teacher_id: teacher_id,
        name: "Professeur Morin".to_string(),
    };

    let second_event = Event::StudentJoined {
        room: "Salle 1".to_string(),
        ts: Utc::now(),
        student_id: Uuid::new_v4(),
        name: "Andre Domingues Ramos".to_string(),
    };

    let third_event = Event::StudentJoined {
        room: "Salle 1".to_string(),
        ts: Utc::now(),
        student_id: Uuid::new_v4(),
        name: "Andre Domingues Ramos 2".to_string(),
    };

    let fourth_event = Event::ClassStarted {
        room: "Salle 1".to_string(),
        ts: Utc::now(),
        teacher_id: teacher_id,
        duration_seconds: 10,
    };

    let fifth_event = Event::StudentJoined {
        room: "Salle 1".to_string(),
        ts: Utc::now(),
        student_id: Uuid::new_v4(),
        name: "Andre Domingues Ramos 3".to_string(),
    };

    send(&mut stream, &first_event);
    send(&mut stream, &second_event);
    send(&mut stream, &third_event);
    send(&mut stream, &fourth_event);

    sleep(Duration::from_secs(11));

    send(&mut stream, &fifth_event);
    println!("Message envoye");
}