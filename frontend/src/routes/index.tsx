import { queryOptions, useSuspenseQuery } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import TimeTable from "@/components/timetable/TimeTable"; // Updated path based on previous file generation
import { fetchTeacherTimetable } from "@/lib/api";

const timetableQuery = queryOptions({
	queryKey: ["teacher-timetable"],
	queryFn: fetchTeacherTimetable,
});

export const Route = createFileRoute("/")({
	component: IndexComponent,
	loader: ({ context: { queryClient } }) =>
		queryClient.ensureQueryData(timetableQuery),
});

function IndexComponent() {
	const { data } = useSuspenseQuery(timetableQuery);
	const today = new Date().toLocaleDateString("en-US", {
		weekday: "long",
		year: "numeric",
		month: "long",
		day: "numeric",
	});

	return (
		<div className="min-h-screen bg-slate-100 p-4 md:p-12 font-sans">
			<div className="max-w-[1400px] mx-auto">
				{/* --- PAGE HEADER SECTION --- */}
				<div className="flex flex-col md:flex-row justify-between items-end mb-8 border-b-8 border-black pb-4 gap-4">
					{/* Left: The Title */}
					<div className="mr-auto md:mr-0">
						<h1 className="text-5xl md:text-7xl font-black text-gray-900 uppercase tracking-tighter leading-none">
							Weekly
							<br />
							Schedule
						</h1>
					</div>

					{/* Right: Date / Context */}
					<div className="text-right">
						<div className="font-bold text-xl uppercase tracking-wide">
							Current_Date
						</div>
						<div className="font-mono text-xl md:text-2xl bg-yellow-300 border-2 border-black px-4 py-1 shadow-[4px_4px_0px_0px_rgba(0,0,0,1)] inline-block">
							{today}
						</div>
					</div>
				</div>

				{/* --- MAIN CONTENT --- */}
				<div className="flex flex-col items-center justify-center">
					<TimeTable classes={data} />

					{/* Optional Footer Text */}
					<p className="mt-6 font-mono text-xs text-gray-700 uppercase tracking-widest">
						* Select a class block to view session details
					</p>
				</div>
			</div>
		</div>
	);
}
