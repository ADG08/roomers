use std::io::Write;
use std::net::TcpStream;
use uuid::Uuid;

fn main() {
    let mut stream = TcpStream::connect("127.0.0.1:9000").expect("impossible de se connecter au serveur");
    
    let teacher_id = Uuid::new_v4();
    let message = format!(
        r#"{{"type":"teacher_joined","room":"Salle 1","ts":"2026-11-09T21:00:00Z","teacher_id":"{}","name":"Professeur Morin"}}"#,
        teacher_id
    );

    stream.write_all(format!("{}\n", message).as_bytes()).expect("envoi impossible");
    println!("Message envoye");
}