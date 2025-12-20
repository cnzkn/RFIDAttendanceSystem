import { useId } from "react";
import { BlockyButton } from "@/components/ui/BlockyButton";
import { BlockyCard } from "@/components/ui/BlockyCard";
import type { SessionDetails } from "@/lib/api";

interface SessionInfoGridProps {
	sessionData: SessionDetails;
	currentRoom: string;
	newRoom: string;
	setNewRoom: (val: string) => void;
	onUpdateRoom: () => void;
	roomSuggestions?: string[];
}

export function SessionInfoGrid({
	sessionData,
	currentRoom,
	newRoom,
	setNewRoom,
	onUpdateRoom,
	roomSuggestions = [],
}: SessionInfoGridProps) {
	const datalistId = useId();

	const formatDate = (dateStr: string) => {
		return new Date(dateStr)
			.toLocaleDateString("en-US", {
				weekday: "long",
				year: "numeric",
				month: "long",
				day: "numeric",
			})
			.toUpperCase();
	};

	const getTimeRange = (slotNum: number) => {
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
		const slot = allTimeSlots.find((s) => s.num === slotNum);
		return slot ? slot.display : `SLOT ${slotNum}`;
	};

	return (
		<BlockyCard className="mb-12 p-0 overflow-hidden">
			{/* Top Grid */}
			<div className="grid grid-cols-1 md:grid-cols-2 divide-y-4 md:divide-y-0 md:divide-x-4 divide-black">
				{/* Course Box */}
				<div className="p-6 bg-white">
					<div className="text-3xl font-black uppercase truncate">
						{sessionData.courseName}
					</div>
					<div className="inline-block bg-black text-white px-2 py-1 mt-2 text-md font-mono">
						SEC: {sessionData.section}
					</div>
				</div>

				{/* Date Box */}
				<div className="p-6 bg-yellow-300">
					<div className="text-3xl font-black font-mono">
						{formatDate(sessionData.date)}
					</div>
					<div className="mt-2 font-bold text-lg flex gap-4">
						<span>WEEK {sessionData.week}</span>
						<span className="font-mono bg-black text-yellow-300 px-2">
							{getTimeRange(sessionData.startHour)}
						</span>
					</div>
				</div>
			</div>

			{/* Room Control Bar */}
			<div className="border-t-4 border-black p-4 bg-gray-50 flex flex-col md:flex-row items-center gap-4">
				<div className="flex-grow flex items-center gap-3 w-full">
					<span className="font-bold whitespace-nowrap uppercase text-sm">
						Current Room:
					</span>
					<span className="font-mono text-xl bg-black text-white px-3 py-1">
						{currentRoom}
					</span>
				</div>
				<div className="flex w-full md:w-auto gap-0 shadow-[4px_4px_0px_0px_rgba(0,0,0,1)]">
					<input
						list={datalistId}
						type="text"
						value={newRoom}
						onChange={(e) => setNewRoom(e.target.value)}
						className="border-2 border-black p-2 w-full md:w-48 focus:outline-none focus:bg-blue-50 font-mono text-sm placeholder-gray-600 uppercase"
						placeholder="NEW ROOM..."
					/>
					<datalist id={datalistId}>
						{roomSuggestions.map((room) => (
							<option key={room} value={room} />
						))}
					</datalist>
					<BlockyButton onClick={onUpdateRoom}>Update</BlockyButton>
				</div>
			</div>
		</BlockyCard>
	);
}
