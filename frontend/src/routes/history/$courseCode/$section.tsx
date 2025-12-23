import {
	queryOptions,
	useQueryClient,
	useSuspenseQuery,
} from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { useState } from "react";
import { BlockyButton } from "@/components/ui/BlockyButton";
import { BlockyCard } from "@/components/ui/BlockyCard";
import {
	fetchCourseHistory,
	saveHistoryChanges,
	exportHistoryCsv,
} from "@/lib/api";

const historyQueryOptions = (courseCode: string, section: string) =>
	queryOptions({
		queryKey: ["history", courseCode, section],
		queryFn: () => fetchCourseHistory(courseCode, section),
	});

export const Route = createFileRoute("/history/$courseCode/$section")({
	component: HistoryComponent,
	loader: ({ context: { queryClient }, params: { courseCode, section } }) =>
		queryClient.ensureQueryData(historyQueryOptions(courseCode, section)),
});

type AttendanceStatus = "present" | "absent" | "pending";

function HistoryComponent() {
	const { courseCode, section } = Route.useParams();
	const { data } = useSuspenseQuery(historyQueryOptions(courseCode, section));
	const queryClient = useQueryClient();

	// Key: "{studentId}-{header}", Value: new status
	const [edits, setEdits] = useState<Record<string, AttendanceStatus>>({});
	const [isSaving, setIsSaving] = useState(false);

	const handleCellClick = (
		studentId: string,
		header: string,
		originalStatus: AttendanceStatus,
		currentStatus: AttendanceStatus,
	) => {
		const nextStatusMap: Record<AttendanceStatus, AttendanceStatus> = {
			pending: "present",
			present: "absent",
			absent: "pending",
		};

		const nextStatus = nextStatusMap[currentStatus];

		setEdits((prev) => {
			const key = `${studentId}-${header}`;
			if (nextStatus === originalStatus) {
				const { [key]: _, ...rest } = prev;
				return rest;
			}
			return { ...prev, [key]: nextStatus };
		});
	};

	const handleSave = async () => {
		if (isSaving) return;
		setIsSaving(true);
		try {
			const updates: import("@/lib/api").HistoryUpdateItem[] = [];

			for (const [key, status] of Object.entries(edits)) {
				// key format: "studentId-wWeek-Day"
				const parts = key.split("-w");
				const studentId = parts[0];
				const weekDayPart = parts[1]; // "Week-Day"
				const [weekStr, dayStr] = weekDayPart.split("-");

				const week = parseInt(weekStr, 10);
				const day = parseInt(dayStr, 10);
				const timetableId = data.timetableIds[day];

				if (timetableId) {
					// Map "pending" to null to indicate deletion
					let apiStatus: "present" | "absent" | null = null;
					if (status === "present") apiStatus = "present";
					else if (status === "absent") apiStatus = "absent";
					// else pending -> null

					updates.push({
						attendeeId: studentId,
						timetableId,
						weekNumber: week,
						status: apiStatus,
					});
				}
			}

			await saveHistoryChanges(courseCode, section, updates);

			// Clear local edits
			setEdits({});
			// Refresh data
			await queryClient.invalidateQueries({
				queryKey: ["history", courseCode, section],
			});
		} catch (error) {
			console.error("Failed to save history:", error);
			alert("Failed to save changes. Please try again.");
		} finally {
			setIsSaving(false);
		}
	};

	const handleExport = async () => {
		try {
			await exportHistoryCsv(courseCode, section);
		} catch (error) {
			console.error("Export failed", error);
			alert("Failed to export CSV");
		}
	};

	const hasChanges = Object.keys(edits).length > 0;

	// Generate headers: w1-1, w1-2, ..., w14-2
	const headers: string[] = [];
	for (let w = 1; w <= data.weeks; w++) {
		for (let d = 1; d <= data.daysPerWeek; d++) {
			headers.push(`w${w}-${d}`);
		}
	}

	return (
		<div className="min-h-screen bg-slate-100 p-4 md:p-8 font-sans">
			<div className="max-w-[95%] mx-auto">
				{/* --- HEADER --- */}
				<div className="flex flex-col md:flex-row justify-between items-end mb-8 border-b-8 border-black pb-4 gap-4">
					<div>
						<div className="bg-black text-white text-xs font-mono px-3 py-1 uppercase tracking-widest inline-block mb-2">
							COURSE #{data.courseCode}
						</div>
						<h1 className="text-4xl md:text-6xl font-black text-gray-900 uppercase tracking-tighter leading-none">
							Attendance
							<br />
							History
						</h1>
					</div>
					<div className="flex flex-col items-end gap-2">
						<div className="font-bold text-xl uppercase tracking-wide">
							Course & Section
						</div>
						<div className="flex items-center gap-2">
							<h2 className="text-2xl font-black uppercase tracking-tight leading-none line-clamp-3">
								{data.courseName}
							</h2>
							<div className="font-black text-base bg-slate-100 px-2 border-2 border-black">
								{data.section}
							</div>
						</div>
						<button
							type="button"
							onClick={handleExport}
							className="text-xs font-bold uppercase tracking-wider border-2 border-blue-600 text-blue-600 px-2 py-1 hover:bg-blue-600 hover:text-white hover:underline transition-all cursor-pointer"
						>
							Export CSV
						</button>
					</div>
				</div>

				{/* --- TABLE CARD --- */}
				<BlockyCard className="p-0 overflow-hidden bg-white mb-8">
					<div className="overflow-x-auto">
						<table className="w-full border-separate border-spacing-0 text-left">
							<thead>
								<tr>
									{/* Sticky Student Name Column */}
									<th className="sticky left-0 top-0 z-30 bg-gray-100 border-b-2 border-r-2 border-black p-3 min-w-[200px] uppercase tracking-wider font-black text-sm shadow-[0px_2px_0px_0px_rgba(0,0,0,0.1)]">
										Student Name
									</th>
									{/* Week Headers */}
									{headers.map((header) => (
										<th
											key={header}
											className="sticky top-0 z-20 border-b-2 border-r-1 px-1 uppercase text-center font-mono font-bold min-w-[60px] bg-gray-50 shadow-[0px_2px_0px_0px_rgba(0,0,0,0.1)]"
										>
											{header}
										</th>
									))}
								</tr>
							</thead>
							<tbody>
								{data.students.map((student) => (
									<tr
										key={student.id}
										className="group hover:bg-yellow-50 transition-colors"
									>
										{/* Sticky Student Name Cell */}
										<td className="sticky left-0 z-10 bg-white border-r-2 border-black border-b border-black p-3 font-bold text-sm uppercase truncate max-w-[200px] group-hover:bg-yellow-50 transition-colors">
											{student.name}
										</td>
										{/* Status Cells */}
										{headers.map((header) => {
											const originalStatus =
												student.attendance[header] || "pending";
											const currentStatus =
												edits[`${student.id}-${header}`] ?? originalStatus;
											const isChanged = currentStatus !== originalStatus;

											let content = "-";
											let cellClass = "text-gray-300";

											if (currentStatus === "present") {
												content = "✓";
												cellClass = "text-green-600 font-black bg-green-50";
											} else if (currentStatus === "absent") {
												content = "✕";
												cellClass = "text-red-600 font-black bg-red-50";
											}

											// Visual feedback for changes
											if (isChanged) {
												cellClass +=
													" ring-4 ring-blue-500 ring-inset z-20 relative";
											}

											return (
												// biome-ignore lint/a11y/useKeyWithClickEvents: <too many cells to actually use keyboard nav>
												<td
													key={`${student.id}-${header}`}
													className={`border-r border-b border-black p-2 text-center font-mono cursor-pointer select-none hover:brightness-95 active:scale-95 transition-all ${cellClass}`}
													onClick={() =>
														handleCellClick(
															student.id,
															header,
															originalStatus,
															currentStatus,
														)
													}
												>
													{content}
												</td>
											);
										})}
									</tr>
								))}
							</tbody>
						</table>
					</div>
				</BlockyCard>

				<div className="mt-6 mb-12 flex justify-between">
					{/* --- LEGEND --- */}
					<div className="flex gap-6 justify-center md:justify-start">
						<div className="flex items-center gap-2">
							<span className="font-black text-green-600">✓</span>
							<span className="text-sm font-bold uppercase">Present</span>
						</div>
						<div className="flex items-center gap-2">
							<span className="font-black text-red-600">✕</span>
							<span className="text-sm font-bold uppercase">Absent</span>
						</div>
						<div className="flex items-center gap-2">
							<span className="font-black text-gray-300">-</span>
							<span className="text-sm font-bold uppercase">Not Set</span>
						</div>
						<div className="flex items-center gap-2">
							<div className="w-4 h-4 border-2 border-blue-500 bg-white"></div>
							<span className="text-sm font-bold uppercase">Changed</span>
						</div>
					</div>
					{/* --- ACTIONS --- */}
					<div className="flex justify-end gap-4">
						{hasChanges && (
							<div className="flex items-center gap-2 text-sm font-bold animate-pulse text-blue-600">
								<span>Unsaved Changes</span>
							</div>
						)}
						<BlockyButton
							onClick={handleSave}
							disabled={!hasChanges || isSaving}
							variant={hasChanges ? "primary" : "neutral"}
							className={
								!hasChanges || isSaving ? "opacity-50 cursor-not-allowed" : ""
							}
						>
							{isSaving ? "Saving..." : "Save Changes"}
						</BlockyButton>
					</div>
				</div>
			</div>
		</div>
	);
}
