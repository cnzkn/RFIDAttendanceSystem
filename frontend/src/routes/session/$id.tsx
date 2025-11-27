import { queryOptions, useSuspenseQuery } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { useEffect, useMemo, useRef, useState } from "react";
import { HeaderCell, type SortKey } from "@/components/session/HeaderCell";
import { SessionInfoGrid } from "@/components/session/SessionInfoGrid";
import { StudentRow } from "@/components/session/StudentRow";
import { BlockyButton } from "@/components/ui/BlockyButton";
import { BlockyCard } from "@/components/ui/BlockyCard";
import {
	createSocketConnection,
	fetchSessionDetails,
	joinSession,
	type Student,
	updateStudentStatus,
	type WSMessage,
} from "@/lib/api";

const sessionQueryOptions = (sessionId: string) =>
	queryOptions({
		queryKey: ["session", sessionId],
		queryFn: () => fetchSessionDetails(sessionId),
	});

export const Route = createFileRoute("/session/$id")({
	component: RouteComponent,
	loader: ({ context: { queryClient }, params: { id } }) =>
		queryClient.ensureQueryData(sessionQueryOptions(id)),
});

function RouteComponent() {
	const { id } = Route.useParams();
	const { data: sessionData } = useSuspenseQuery(sessionQueryOptions(id));

	// --- Local State ---
	const [students, setStudents] = useState<Student[]>([]);
	const [currentRoom, setCurrentRoom] = useState(sessionData.room || "");
	const [newRoom, setNewRoom] = useState("");
	const socketRef = useRef<WebSocket | null>(null);

	// --- Sorting State ---
	const [sortConfig, setSortConfig] = useState<{
		key: SortKey;
		direction: "asc" | "desc";
	}>({
		key: "name",
		direction: "asc",
	});

	// --- Sorting Logic ---
	const sortedStudents = useMemo(() => {
		const sorted = [...students];
		sorted.sort((a, b) => {
			let aValue: string | number = "";
			let bValue: string | number = "";

			switch (sortConfig.key) {
				case "name":
					aValue = a.name.toLowerCase();
					bValue = b.name.toLowerCase();
					break;
				case "status": {
					const statusPriority = { present: 1, absent: 2, nothing: 3 };
					aValue = statusPriority[a.status || "nothing"] || 3;
					bValue = statusPriority[b.status || "nothing"] || 3;
					break;
				}
				case "time":
					aValue = a.timestamp ? new Date(a.timestamp).getTime() : 0;
					bValue = b.timestamp ? new Date(b.timestamp).getTime() : 0;
					break;
			}

			if (aValue < bValue) return sortConfig.direction === "asc" ? -1 : 1;
			if (aValue > bValue) return sortConfig.direction === "asc" ? 1 : -1;
			return 0;
		});
		return sorted;
	}, [students, sortConfig]);

	const handleSort = (key: SortKey) => {
		setSortConfig((current) => ({
			key,
			direction:
				current.key === key && current.direction === "asc" ? "desc" : "asc",
		}));
	};

	// --- Socket Logic ---
	useEffect(() => {
		if (sessionData.room) setCurrentRoom(sessionData.room);
	}, [sessionData.room]);

	useEffect(() => {
		const socket = createSocketConnection();
		socketRef.current = socket;

		socket.onopen = () => {
			joinSession(socket, id);
		};

		socket.onmessage = (event) => {
			try {
				const data = JSON.parse(event.data) as WSMessage;

				if (data.type === "initial_list") {
					setStudents(data.students);
				} else if (data.type === "student_updated") {
					// Update local state based on the specific student ID
					setStudents((prev) =>
						prev.map((s) =>
							s.studentId === data.studentId
								? {
										...s,
										status: data.status as "present" | "absent" | "nothing",
										timestamp: data.timestamp,
										isManual: data.isManual,
									}
								: s,
						),
					);
				}
			} catch (err) {
				console.error("Failed to parse websocket message:", err);
			}
		};

		// 4. Cleanup
		return () => {
			socket.close();
			socketRef.current = null;
		};
	}, [id]);

	const handleStatusChange = (
		studentId: string,
		status: "present" | "absent",
	) => {
		if (socketRef.current)
			updateStudentStatus(socketRef.current, id, studentId, status);
	};

	const handleRoomChange = () => {
		if (newRoom.trim()) {
			setCurrentRoom(newRoom);
			setNewRoom("");
			alert("Room updated locally");
		}
	};

	return (
		<div className="min-h-screen bg-slate-100 p-4 md:p-8 font-sans">
			<div className="max-w-5xl mx-auto">
				<SessionInfoGrid
					sessionData={sessionData}
					currentRoom={currentRoom}
					newRoom={newRoom}
					setNewRoom={setNewRoom}
					onUpdateRoom={handleRoomChange}
				/>

				<div className="mb-6 flex justify-between items-end">
					<h2 className="text-3xl font-black text-gray-800 uppercase">
						Attendance List{" "}
						<span className="text-gray-400">({students.length})</span>
					</h2>
				</div>

				<BlockyCard className="p-0 bg-white">
					{students.length > 0 ? (
						<div className="grid grid-cols-1 md:grid-cols-[1fr_300px_140px]">
							<div
								className="col-span-1 md:col-span-3 grid grid-cols-subgrid gap-4 bg-gray-100 p-3 
                                        font-bold text-sm uppercase tracking-wider border-b-4 border-black"
							>
								<HeaderCell
									label="Student Details"
									sortKey="name"
									currentSort={sortConfig}
									onSort={handleSort}
									className=""
								/>
								<HeaderCell
									label="Status Action"
									sortKey="status"
									currentSort={sortConfig}
									onSort={handleSort}
									className=""
								/>
								<HeaderCell
									label="Log Time"
									sortKey="time"
									currentSort={sortConfig}
									onSort={handleSort}
									className=""
								/>
							</div>

							{/* ROWS */}
							{sortedStudents.map((student) => (
								<StudentRow
									key={student.studentId}
									student={student}
									onStatusChange={handleStatusChange}
								/>
							))}
						</div>
					) : (
						<div className="p-12 text-center text-gray-500 font-bold uppercase tracking-widest">
							Waiting for connection...
						</div>
					)}
				</BlockyCard>

				<div className="text-center mt-12 mb-12">
					<BlockyButton
						variant="primary"
						size="lg"
						className="bg-purple-600 hover:bg-purple-700"
						onClick={() => alert("Navigating...")}
					>
						View Full History
					</BlockyButton>
				</div>
			</div>
		</div>
	);
}
