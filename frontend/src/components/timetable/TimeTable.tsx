import { ClassCard } from "@/components/timetable/ClassCard";
import { TimeSlotLabel } from "@/components/timetable/TimeSlotLabel";
import type { TimetableEntry } from "@/lib/api";

// --- Configuration Data ---
const allTimeSlots = [
	{ num: 1, display: "08:40 - 09:30" },
	{ num: 2, display: "09:40 - 10:30" },
	{ num: 3, display: "10:40 - 11:30" },
	{ num: 4, display: "11:40 - 12:30" },
	{ num: 5, display: "12:40 - 13:30" },
	{ num: 6, display: "13:40 - 14:30" },
	{ num: 7, display: "14:40 - 15:30" },
	{ num: 8, display: "15:40 - 16:30" },
	{ num: 9, display: "16:40 - 17:30" },
	{ num: 10, display: "17:40 - 18:30" },
	{ num: 11, display: "18:40 - 19:30" },
	{ num: 12, display: "19:40 - 20:30" },
];

const daysOfWeek = [
	{ num: 1, name: "MON" },
	{ num: 2, name: "TUE" },
	{ num: 3, name: "WED" },
	{ num: 4, name: "THU" },
	{ num: 5, name: "FRI" },
	{ num: 6, name: "SAT" },
	{ num: 7, name: "SUN" },
];

// --- Helper Logic ---
const getClassForSlot = (
	classes: TimetableEntry[],
	dayOfWeek: number,
	timeslotNumber: number,
): TimetableEntry | undefined => {
	return classes.find(
		(cls) =>
			cls.dayOfWeek === dayOfWeek &&
			cls.startHour <= timeslotNumber &&
			cls.endHour >= timeslotNumber,
	);
};

// --- Components ---
const EmptyCell = () => (
	<div className="w-full h-full opacity-[0.05] bg-[radial-gradient(#000_1px,transparent_1px)] [background-size:4px_4px]" />
);

export default function TimeTable({ classes }: { classes: TimetableEntry[] }) {
	return (
		<div className="w-full max-w-[1200px] bg-white border-4 border-black shadow-[8px_8px_0px_0px_rgba(0,0,0,1)]">
			<div className="overflow-x-auto">
				<table className="w-full min-w-[1000px] border-collapse table-fixed">
					{/* --- TABLE HEAD --- */}
					<thead>
						<tr>
							<th className="w-24 border-b-4 border-r-4 border-black bg-black text-white p-2">
								<div className="uppercase tracking-widest text-center opacity-80">
									Time
								</div>
							</th>
							{daysOfWeek.map((day) => (
								<th
									key={day.num}
									className="border-b-4 border-r-2 border-black bg-yellow-300 text-black p-3 last:border-r-0"
								>
									<div className="font-black text-xl uppercase tracking-tighter text-center">
										{day.name}
									</div>
								</th>
							))}
						</tr>
					</thead>

					{/* --- TABLE BODY --- */}
					<tbody>
						{allTimeSlots.map((slot) => (
							<tr key={slot.num}>
								{/* Time Label Column */}
								<td className="h-24 border-b-2 border-r-4 border-black bg-gray-50 p-2 transition-colors">
									<TimeSlotLabel display={slot.display} />
								</td>

								{/* Day Columns */}
								{daysOfWeek.map((day) => {
									const entry = getClassForSlot(classes, day.num, slot.num);

									return (
										<td
											key={day.num}
											className="h-24 border-b-2 border-r-2 border-black bg-white last:border-r-0 p-1 align-top"
										>
											{entry ? <ClassCard entry={entry} /> : <EmptyCell />}
										</td>
									);
								})}
							</tr>
						))}
					</tbody>
				</table>
			</div>
		</div>
	);
}
