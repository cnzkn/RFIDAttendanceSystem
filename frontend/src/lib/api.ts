import { toUITimetable, toUISession, toUIHistory } from "./adapters";

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
	courseCode: string;
	section: string;
	date: string;
	room: string;
	week: number;
	day: number;
	startHour: number;
}

export interface TimetableEntry {
	id: string;
	course: string;
	courseCode: string;
	section: string;
	room: string;
	dayOfWeek: number;
	startHour: number;
	endHour: number;
}

// --- History Types ---
export interface HistoryStudent {
	id: string;
	name: string;
	attendance: Record<string, "present" | "absent" | "pending">;
}

export interface CourseHistory {
	courseName: string;
	courseCode: string;
	section: string;
	students: HistoryStudent[];
	weeks: number;
	daysPerWeek: number;
    timetableIds: Record<number, string>;
}

// --- Backend Types ---
export interface BackendCourse {
	code: number;
	name: string;
}

export interface BackendUser {
	fullName: string;
	userName: string;
	role: number | string;
}

export interface BackendSection {
	course: BackendCourse;
	section: string;
	user: BackendUser;
}

export interface BackendClassroom {
	name: string;
}

export interface BackendTimeslot {
	dayOfWeek: number;
	timeslotNumber: number;
}

export interface BackendTimetableEntry {
	id: string;
	section: BackendSection;
	classroom: BackendClassroom;
	timeslot: BackendTimeslot;
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
const API_BASE_URL = import.meta.env.VITE_API_URL || "";

// Helper to convert HTTP URL to WS URL
function getWebSocketURL(url: string): string {
	let wsUrl = url;
	if (!url || url.startsWith("/")) {
		const protocol = window.location.protocol === "https:" ? "wss:" : "ws:";
		const host = window.location.host;
		// If url is empty, it means root. If it's relative, append it.
		wsUrl = `${protocol}//${host}${url}`;
	} else {
		wsUrl = url.replace(/^http/, "ws");
	}

	// Ensure it points to the /ws/client endpoint
	if (!wsUrl.endsWith("/ws/client")) {
		wsUrl = `${wsUrl.replace(/\/$/, "")}/ws/client`;
	}
	return wsUrl;
}

// --- Auth Functions ---
export async function logIn(userName: string, password: string) {
	console.log("API: logIn called with", { userName });

	const response = await fetch(`${API_BASE_URL}/User/login`, {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
		},
		body: JSON.stringify({ userName, password }),
		credentials: "include",
	});

	if (!response.ok) {
		console.error("API: logIn failed", response.status);
		throw new Error("Login failed");
	}

	console.log("API: logIn success");
}

export async function logOut() {
	console.log("API: logOut called");

	await fetch(`${API_BASE_URL}/User/logout`, {
		method: "POST",
		credentials: "include",
	});

	console.log("API: logOut success");
}

// --- Data Fetching Functions ---
export async function fetchTeacherTimetable(): Promise<TimetableEntry[]> {
	console.log("API: fetchTeacherTimetable called");

	try {
		const response = await fetch(`${API_BASE_URL}/User/timetable`, {
			method: "GET",
			credentials: "include",
			headers: {
				"Content-Type": "application/json",
			},
		});

		if (!response.ok) {
			console.error("API: fetchTeacherTimetable failed", response.status);
			throw new Error(`HTTP error! status: ${response.status}`);
		}

		const data: BackendTimetableEntry[] = await response.json();
		console.log("API: fetchTeacherTimetable success", data);

		// Map backend data to frontend TimetableEntry
		return data.map(toUITimetable);
	} catch (error) {
		console.error("Failed to fetch timetable:", error);
		return [];
	}
}

export async function fetchUserSections(): Promise<BackendSection[]> {
	console.log("API: fetchUserSections called");

	const response = await fetch(`${API_BASE_URL}/User/sections`, {
		credentials: "include",
	});

	if (!response.ok) {
		console.error("API: fetchUserSections failed", response.status);
		throw new Error("Failed to fetch user sections");
	}

	const data = await response.json();
	console.log("API: fetchUserSections success", data);

	return data;
}

export async function fetchCurrentUser(): Promise<BackendUser> {
	console.log("API: fetchCurrentUser called");

	const response = await fetch(`${API_BASE_URL}/User`, {
		credentials: "include",
	});

	if (!response.ok) {
		console.error("API: fetchCurrentUser failed", response.status);
		throw new Error("Not authenticated");
	}

	const data = await response.json();
	console.log("API: fetchCurrentUser success", data);

	return data;
}

// --- Session Functions ---
export async function fetchSessionDetails(
	sessionId: string,
): Promise<SessionDetails> {
	console.log("API: fetchSessionDetails called with", sessionId);

	const response = await fetch(`${API_BASE_URL}/Session/${sessionId}`, {
		credentials: "include",
	});

	if (!response.ok) {
		console.error("API: fetchSessionDetails failed", response.status);
		throw new Error("Failed to load session details");
	}

	const data = await response.json();
	console.log("API: fetchSessionDetails success", data);

	return toUISession(data);
}

export async function fetchCourseHistory(
	courseCode: string,
	section: string,
): Promise<CourseHistory> {
	console.log("API: fetchCourseHistory called", { courseCode, section });

	const response = await fetch(
		`${API_BASE_URL}/Attendance/history/${courseCode}/${section}`,
		{
			credentials: "include",
		},
	);

	if (!response.ok) {
		console.error("API: fetchCourseHistory failed", response.status);
		throw new Error("Failed to load course history");
	}

	const data = await response.json();
	console.log("API: fetchCourseHistory success", data);

	return toUIHistory(data);
}

// --- WebSocket Helpers ---
export function createSocketConnection(): WebSocket {
	const wsUrl = getWebSocketURL(API_BASE_URL);
	console.log("API: Connecting to WebSocket", wsUrl);
	const socket = new WebSocket(wsUrl);
	socket.onopen = () => console.log("API: WebSocket connected");
	socket.onclose = () => console.log("API: WebSocket disconnected");
	socket.onerror = (err) => console.error("API: WebSocket error", err);
	return socket;
}

export function joinSession(socket: WebSocket, sessionId: string) {
	console.log("API: joinSession called for", sessionId);
	const payload = {
		action: "join_session",
		attendanceSessionId: sessionId,
	};

	if (socket.readyState === WebSocket.OPEN) {
		console.log("API: Sending join_session payload immediately");
		socket.send(JSON.stringify(payload));
	} else {
		console.log("API: Waiting for socket open to join_session");
		socket.addEventListener(
			"open",
			() => {
				console.log("API: Socket opened, sending join_session");
				socket.send(JSON.stringify(payload));
			},
			{ once: true },
		);
	}
}

export async function updateStudentStatus(
	sessionId: string,
	studentId: string,
	status: "present" | "absent",
) {
	console.log("API: updateStudentStatus called", {
		sessionId,
		studentId,
		status,
	});

	const response = await fetch(`${API_BASE_URL}/Attendance/update`, {
		method: "POST",
		headers: {
			"Content-Type": "application/json",
		},
		body: JSON.stringify({ sessionId, studentId, status }),
		credentials: "include",
	});

	if (!response.ok) {
		console.error("API: updateStudentStatus failed", response.status);
		throw new Error("Failed to update status");
	}

	console.log("API: updateStudentStatus success");
}

export async function updateSessionRoom(sessionId: string, roomName: string) {
	console.log("API: updateSessionRoom called", { sessionId, roomName });

	const response = await fetch(
		`${API_BASE_URL}/Session/update-room/${sessionId}`,
		{
			method: "POST",
			headers: {
				"Content-Type": "application/json",
			},
			body: JSON.stringify({ roomName }),
			credentials: "include",
		},
	);

	if (!response.ok) {
		console.error("API: updateSessionRoom failed", response.status);
		throw new Error("Failed to update room");
	}

	console.log("API: updateSessionRoom success");
}

// --- Admin / Device Functions ---
export interface BackendClassroom {
	id: string;
	name: string;
}

export interface BackendDevice {
	id: string;
	classroom: BackendClassroom;
	fingerprint: string;
}

export interface CreateDeviceRequest {
	fingerprint: string;
	classroomId: string;
}

export async function fetchDevices(): Promise<BackendDevice[]> {
	console.log("API: fetchDevices called");
	const response = await fetch(`${API_BASE_URL}/Device`, {
		credentials: "include",
	});
	if (!response.ok) throw new Error("Failed to fetch devices");
	return response.json();
}

export async function fetchClassrooms(): Promise<BackendClassroom[]> {
	console.log("API: fetchClassrooms called");
	const response = await fetch(`${API_BASE_URL}/Classroom`, {
		credentials: "include",
	});
	if (!response.ok) throw new Error("Failed to fetch classrooms");
	return response.json();
}

export async function createDevice(
	data: CreateDeviceRequest,
): Promise<BackendDevice> {
	console.log("API: createDevice called", data);
	const response = await fetch(`${API_BASE_URL}/Device`, {
		method: "POST",
		headers: { "Content-Type": "application/json" },
		body: JSON.stringify(data),
		credentials: "include",
	});
	if (!response.ok) {
		const msg = await response.text();
		throw new Error(msg || "Failed to create device");
	}
	return response.json();
}

export async function updateDevice(
	id: string,
	data: CreateDeviceRequest,
): Promise<BackendDevice> {
	console.log("API: updateDevice called", id, data);
    // Transform frontend "CreateDeviceRequest" to backend "UpdateDeviceRequestDto"
    const payload = {
        newFingerprint: data.fingerprint ? data.fingerprint : null, // Handle potential empty string if logic requires
        newClassroomId: data.classroomId
    };

	const response = await fetch(`${API_BASE_URL}/Device/${id}`, {
		method: "PUT",
		headers: { "Content-Type": "application/json" },
		body: JSON.stringify(payload),
		credentials: "include",
	});
	if (!response.ok) {
		const msg = await response.text();
		throw new Error(msg || "Failed to update device");
	}
	return response.json();
}

export async function deleteDevice(id: string) {
	console.log("API: deleteDevice called", id);
	const response = await fetch(`${API_BASE_URL}/Device/${id}`, {
		method: "DELETE",
		credentials: "include",
	});
	if (!response.ok) throw new Error("Failed to delete device");
}

export interface HistoryUpdateItem {
	attendeeId: string;
    timetableId: string;
    weekNumber?: number;
	status: "present" | "absent" | null;
}

export async function saveHistoryChanges(
	courseCode: string,
	section: string,
	updates: HistoryUpdateItem[],
) {
	console.log("API: saveHistoryChanges called", {
		courseCode,
		section,
		updates,
	});

	const response = await fetch(
		`${API_BASE_URL}/Attendance/history/update/${courseCode}/${section}`,
		{
			method: "POST",
			headers: {
				"Content-Type": "application/json",
			},
			body: JSON.stringify({ updates }),
			credentials: "include",
		},
	);

	if (!response.ok) {
		console.error("API: saveHistoryChanges failed", response.status);
		throw new Error("Failed to save history changes");
	}

	console.log("API: saveHistoryChanges success");
}
