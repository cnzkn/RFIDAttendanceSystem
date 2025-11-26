interface TimeSlotLabelProps {
	display: string;
}

export function TimeSlotLabel({ display }: TimeSlotLabelProps) {
	const [start, end] = display.split(" - ");

	return (
		<div className="flex flex-col items-center justify-center gap-1 h-full w-full text-lg font-bold font-mono leading-2">
			<span className="text-black">{start}</span>
			<span className="text-gray-600 text-sm">↓</span>
			<span className="text-gray-700">{end}</span>
		</div>
	);
}
