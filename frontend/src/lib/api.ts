export function logOut() {
	localStorage.removeItem("token");
}

export function logIn(username: string, password: string) {
	localStorage.setItem("token", `${username}:${password}`);
}

// --- Types ---
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
	id: string;
	course: string;
	section: string;
	room: string;
	dayOfWeek: number;
	startHour: number;
	endHour: number;
}

// --- WebSocket Message Types ---
export type WSMessage =
	| { type: "initial_list"; attendanceSessionId: string; students: Student[] }
	| {
			type: "student_updated";
			studentId: string;
			status: string;
			timestamp: string | null;
			isManual: boolean;
	  };

// --- Configuration ---

const API_BASE_URL = import.meta.env.VITE_API_URL || "http://127.0.0.1:5000";

// Helper to convert HTTP URL to WS URL
function getWebSocketURL(url: string): string {
	// Replace http/https with ws/wss
	let wsUrl = url.replace(/^http/, "ws");
	// Ensure it points to the /ws endpoint defined in Go
	if (!wsUrl.endsWith("/ws")) {
		wsUrl = `${wsUrl.replace(/\/$/, "")}/ws`;
	}
	return wsUrl;
}

// --- HTTP API Functions ---

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

// Factory to create a native WebSocket connection
export function createSocketConnection(): WebSocket {
	const wsUrl = getWebSocketURL(API_BASE_URL);
	const socket = new WebSocket(wsUrl);
	return socket;
}

// Helper to join a specific session room
// In Native WS, we send a JSON string with an "action" field
export function joinSession(socket: WebSocket, sessionId: string) {
	const payload = {
		action: "join_session",
		attendanceSessionId: sessionId,
	};

	// If the socket is already open, send immediately
	if (socket.readyState === WebSocket.OPEN) {
		socket.send(JSON.stringify(payload));
	} else {
		// If not, wait for it to open
		socket.addEventListener(
			"open",
			() => {
				socket.send(JSON.stringify(payload));
			},
			{ once: true },
		);
	}
}

// Helper to update attendance
export function updateStudentStatus(
	socket: WebSocket,
	sessionId: string,
	studentId: string,
	status: "present" | "absent",
) {
	const payload = {
		action: "update_attendance",
		attendanceSessionId: sessionId,
		studentId: studentId,
		status: status.toLowerCase(),
		timestamp: null, // Indicates manual update, Go handles logic
	};

	if (socket.readyState === WebSocket.OPEN) {
		socket.send(JSON.stringify(payload));
	}
}
