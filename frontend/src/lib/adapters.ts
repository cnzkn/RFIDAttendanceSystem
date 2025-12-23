import type {
	BackendTimetableEntry,
	TimetableEntry,
	SessionDetails,
	BackendTimeslot,
	CourseHistory,
	HistoryStudent,
} from "./api";

// --- Types needed for adapters but not exported from api.ts yet ---

export interface BackendSessionWrapper {
	session: BackendTimetableEntry;
	currentWeek: number;
}

export interface AttendanceHistorySessionDto {
	weekNumber: number;
	attendance: Record<string, string[]>; // "Present" | "Absent" -> List of Student IDs
}

export interface AttendanceTimetableDto {
	id: string;
	timeslot: BackendTimeslot;
	sessions: AttendanceHistorySessionDto[];
}

export interface BackendAttendanceHistoryDto {
	section: {
		id: string;
		course: { id: string; code: number; name: string };
		section: string;
		user: any;
	};
	students: { id: string; fullName: string; studentID: string }[];
	timetables: AttendanceTimetableDto[];
}

// --- Helpers ---

/**
 * Maps Backend DayOfWeek (string or 0-6 number) to Frontend 1-7 (Mon=1, Sun=7)
 * C# DayOfWeek: Sunday=0, Monday=1, ..., Saturday=6
 */
function toDayNum(day: string | number): number {
	const dayMap: Record<string, number> = {
		Monday: 1,
		Tuesday: 2,
		Wednesday: 3,
		Thursday: 4,
		Friday: 5,
		Saturday: 6,
		Sunday: 7,
	};

	if (typeof day === "string") {
		return dayMap[day] || 0;
	}

	// If it's a number (0-6)
	if (day === 0) return 7; // Sunday 0 -> 7
	return day; // 1-6 match Mon-Sat
}

// --- Adapters ---

export function toUITimetable(
	backendEntry: BackendTimetableEntry,
): TimetableEntry {
	return {
		id: backendEntry.id,
		course: backendEntry.section.course.name,
		courseCode: backendEntry.section.course.code.toString(),
		section: backendEntry.section.section,
		room: backendEntry.classroom.name,
		dayOfWeek: toDayNum(backendEntry.timeslot.dayOfWeek),
		startHour: backendEntry.timeslot.timeslotNumber,
		endHour: backendEntry.timeslot.timeslotNumber, // Assuming 1 hour slot for now
	};
}

export function toUISession(wrapper: BackendSessionWrapper): SessionDetails {
	const { session, currentWeek } = wrapper;

	// Date Calculation
	const today = new Date();
	const currentDayOfWeek = today.getDay() === 0 ? 7 : today.getDay(); // JS 0=Sun -> 7
	const targetDayOfWeek = toDayNum(session.timeslot.dayOfWeek);

	const dayDiff = targetDayOfWeek - currentDayOfWeek;
	const targetDate = new Date(today);
	targetDate.setDate(today.getDate() + dayDiff);

	const dateStr = targetDate.toISOString().split("T")[0];

	return {
		attendanceSessionId: session.id,
		courseName: session.section.course.name,
		courseCode: session.section.course.code.toString(),
		section: session.section.section,
		date: dateStr,
		room: session.classroom.name,
		week: currentWeek,
		day: targetDayOfWeek,
		startHour: session.timeslot.timeslotNumber,
	};
}

export function toUIHistory(
	backendHistoryList: BackendAttendanceHistoryDto[],
): CourseHistory {
	if (!backendHistoryList || backendHistoryList.length === 0) {
		return {
			courseName: "",
			courseCode: "",
			section: "",
			students: [],
			weeks: 0,
			daysPerWeek: 0,
			timetableIds: {},
		};
	}

	const sectionDto = backendHistoryList[0];
	const courseName = sectionDto.section.course.name;
	const courseCode = sectionDto.section.course.code.toString();
	const sectionName = sectionDto.section.section;

	const sortedTimetables = [...sectionDto.timetables].sort((a, b) => {
		const da = toDayNum(a.timeslot.dayOfWeek);
		const db = toDayNum(b.timeslot.dayOfWeek);
		if (da !== db) return da - db;
		return a.timeslot.timeslotNumber - b.timeslot.timeslotNumber;
	});

	const daysPerWeek = sortedTimetables.length;
	let maxWeek = 0;

	const studentMap = new Map<string, HistoryStudent>();
	const timetableIds: Record<number, string> = {}; // Map DayIndex -> TimetableId

	const lookup = new Map<string, { id: string; fullName: string }>();
	sectionDto.students.forEach((s) => {
		lookup.set(s.id, s);
		studentMap.set(s.id, {
			id: s.id,
			name: s.fullName,
			attendance: {},
		});
	});

	sortedTimetables.forEach((timetable, index) => {
		const dayIdx = index + 1;
		timetableIds[dayIdx] = timetable.id;

		timetable.sessions.forEach((session) => {
			if (session.weekNumber > maxWeek) maxWeek = session.weekNumber;
			const weekIdx = session.weekNumber;
			const key = `w${weekIdx}-${dayIdx}`;

			const presentList = session.attendance.Present || [];
			presentList.forEach((studentId) => {
				const st = studentMap.get(studentId);
				if (st) st.attendance[key] = "present";
			});

			const absentList = session.attendance.Absent || [];
			absentList.forEach((studentId) => {
				const st = studentMap.get(studentId);
				if (st && st.attendance[key] !== "present") {
					st.attendance[key] = "absent";
				}
			});
		});
	});

	return {
		courseName,
		courseCode,
		section: sectionName,
		students: Array.from(studentMap.values()).sort((a, b) =>
			a.name.localeCompare(b.name),
		),
		weeks: maxWeek,
		daysPerWeek,
		timetableIds,
	};
}
