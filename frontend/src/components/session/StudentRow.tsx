import { BlockyButton } from "@/components/ui/BlockyButton";
import type { Student } from "@/lib/api";

interface StudentRowProps {
	student: Student;
	onStatusChange: (id: string, status: "present" | "absent") => void;
}

export function StudentRow({ student, onStatusChange }: StudentRowProps) {
	const isPresent = student.status === "present";
	const isAbsent = student.status === "absent";

	// Row background logic
	const rowBg = isPresent ? "bg-green-50" : isAbsent ? "bg-red-50" : "bg-white";

	return (
		<div
			className={`col-span-1 md:col-span-3 grid grid-cols-subgrid 
                    gap-4 p-4 items-center transition-colors duration-200 
                    border-b-2 border-black last:border-b-0 ${rowBg}`}
		>
			{/* Student Info */}
			<div className="flex flex-col">
				<span className="font-bold text-lg leading-tight uppercase">
					{student.name}
				</span>
				<span className="font-mono text-xs text-gray-600 mt-1">
					{student.studentId}
				</span>
			</div>

			{/* Buttons */}
			<div className="flex gap-3">
				<BlockyButton
					onClick={() => onStatusChange(student.studentId, "present")}
					variant={isPresent ? "success" : "neutral"}
					className={!isPresent ? "text-gray-400" : ""}
					size="md"
				>
					Present
				</BlockyButton>

				<BlockyButton
					onClick={() => onStatusChange(student.studentId, "absent")}
					variant={isAbsent ? "danger" : "neutral"}
					className={!isAbsent ? "text-gray-400" : ""}
					size="md"
				>
					Absent
				</BlockyButton>
			</div>

			{/* Timestamp */}
			<div className="flex items-center justify-start md:justify-center">
				{student.timestamp ? (
					<div
						className="font-mono text-xs border-2 border-black bg-white px-2 py-1 
                                    shadow-[2px_2px_0px_0px_rgba(0,0,0,1)]"
					>
						{new Date(student.timestamp).toLocaleTimeString([], {
							hour: "2-digit",
							minute: "2-digit",
						})}
					</div>
				) : (
					<span className="text-xs font-bold text-gray-900 uppercase">
						--:--
					</span>
				)}
			</div>
		</div>
	);
}
