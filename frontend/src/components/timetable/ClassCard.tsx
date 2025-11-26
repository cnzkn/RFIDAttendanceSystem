import { Link } from "@tanstack/react-router";
import type { TimetableEntry } from "@/lib/api";

interface ClassCardProps {
	entry: TimetableEntry;
}

export function ClassCard({ entry }: ClassCardProps) {
	return (
		<Link
			to={"/session/$id"}
			params={{ id: entry.attendanceSessionId }}
			className=" group relative block w-full h-full border-2 border-black bg-blue-400 transition-all duration-200 ease-out
                        overflow-hidden shadow-[3px_3px_0px_0px_rgba(0,0,0,1)] hover:shadow-[5px_5px_0px_0px_rgba(0,0,0,1)] 
                        hover:translate-x-[-1px] hover:translate-y-[-1px] hover:bg-blue-500 flex flex-col justify-between p-2"
		>
			<div className="absolute top-0 left-0 w-full h-1 bg-black opacity-20" />

			<div className="font-black text-xl uppercase leading-tight break-words z-10 group-hover:text-white transition-colors">
				{entry.courseName}
			</div>

			<div className="flex justify-between items-end mt-2 z-10 text-lg">
				<span className="font-bold bg-white text-black px-1 border border-black group-hover:bg-black group-hover:text-white transition-colors">
					{entry.section}
				</span>
				<span className="font-bold text-black opacity-80 group-hover:text-white group-hover:opacity-100 transition-opacity">
					{entry.room}
				</span>
			</div>
		</Link>
	);
}
