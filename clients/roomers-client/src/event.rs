use chrono::{DateTime, Utc};
use serde::Serialize;
use uuid::Uuid;

#[derive(Serialize)]
#[serde(tag = "type", rename_all = "snake_case")]
pub enum Event {
    TeacherJoined { room: String, ts: DateTime<Utc>, teacher_id: Uuid, name: String },
    TeacherLeft { room: String, ts: DateTime<Utc>, teacher_id: Uuid },
    StudentJoined { room: String, ts: DateTime<Utc>, student_id: Uuid, name: String },
    StudentLeft { room: String, ts: DateTime<Utc>, student_id: Uuid },
    StudentActive { room: String, ts: DateTime<Utc>, student_id: Uuid },
    ClassStarted { room: String, ts: DateTime<Utc>, teacher_id: Uuid, duration_seconds: u32 },
    ClassEnded { room: String, ts: DateTime<Utc>, teacher_id: Uuid },
}