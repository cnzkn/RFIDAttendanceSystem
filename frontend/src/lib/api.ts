import { io, type Socket } from "socket.io-client";

export function logOut() {
	localStorage.removeItem("token");
}

export function logIn(username: string, password: string) {
	localStorage.setItem("token", `${username}:${password}`);
}

export interface Student {
	studentId: string;
	name: string;
	status: "present" | "absent" | "nothing";
	timestamp: string | null;
	isManual: boolean;
}

export interface SessionDetails {
	attendanceSessionId: string;
	courseName: string;
	section: string;
	date: string;
	room: string;
	week: number;
	day: number;
}

export interface TimetableEntry {
	attendanceSessionId: string;
	courseName: string;
	section: string;
	room: string;
	dayOfWeek: number;
	startHour: number;
	endHour: number;
}

const API_BASE_URL = import.meta.env.VITE_API_URL || "http://127.0.0.1:5000";

export async function fetchTeacherTimetable(): Promise<TimetableEntry[]> {
	try {
		const response = await fetch(`${API_BASE_URL}/api/timetable`, {
			method: "GET",
			headers: {
				"Content-Type": "application/json",
			},
		});

		if (!response.ok) {
			throw new Error(`HTTP error! status: ${response.status}`);
		}

		const data: TimetableEntry[] = await response.json();
		return data;
	} catch (error) {
		console.error("Failed to fetch timetable:", error);
		return [];
	}
}

export async function fetchSessionDetails(
	sessionId: string,
): Promise<SessionDetails> {
	const response = await fetch(`${API_BASE_URL}/api/session/${sessionId}`);
	if (!response.ok) {
		throw new Error("Failed to load session details");
	}
	return response.json();
}

// --- WebSocket Helpers ---

// Factory to create a connection
export function createSocketConnection(): Socket {
	return io(API_BASE_URL, {
		autoConnect: false, // Better control over when it starts
	});
}

// Helper to join a specific session room
export function joinSession(socket: Socket, sessionId: string) {
	if (socket.connected) {
		socket.emit("join_session", { attendanceSessionId: sessionId });
	}
}

// Helper to update attendance
export function updateStudentStatus(
	socket: Socket,
	sessionId: string,
	studentId: string,
	status: "present" | "absent",
) {
	socket.emit("update_attendance", {
		attendanceSessionId: sessionId,
		studentId: studentId,
		status: status.toLowerCase(),
		timestamp: null, // Indicates manual update
	});
}
