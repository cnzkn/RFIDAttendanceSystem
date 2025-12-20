import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { createFileRoute } from "@tanstack/react-router";
import { useId, useMemo, useState } from "react";
import { BlockyButton } from "@/components/ui/BlockyButton";
import { BlockyCard } from "@/components/ui/BlockyCard";
import { BlockyModal } from "@/components/ui/BlockyModal";
import {
	type CreateDeviceRequest,
	createDevice,
	deleteDevice,
	fetchClassrooms,
	fetchDevices,
	updateDevice,
} from "@/lib/api";

export const Route = createFileRoute("/admin/devices")({
	component: AdminDevicesComponent,
});

function AdminDevicesComponent() {
	const queryClient = useQueryClient();
	const [isModalOpen, setIsModalOpen] = useState(false);
	const [editingId, setEditingId] = useState<string | null>(null);

	// Form State
	const [formData, setFormData] = useState<CreateDeviceRequest>({
		fingerprint: "",
		classroomId: "",
	});

	// --- Queries ---
	const { data: devices = [], isLoading: loadingDevices } = useQuery({
		queryKey: ["devices"],
		queryFn: fetchDevices,
	});

	const sortedDevices = useMemo(() => {
		return [...devices].sort((a, b) =>
			a.fingerprint.localeCompare(b.fingerprint),
		);
	}, [devices]);

	const { data: classrooms = [] } = useQuery({
		queryKey: ["classrooms"],
		queryFn: fetchClassrooms,
	});

	// --- Mutations ---
	const createMutation = useMutation({
		mutationFn: createDevice,
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: ["devices"] });
			closeModal();
		},
		onError: (err) => alert(err.message),
	});

	const updateMutation = useMutation({
		mutationFn: (vars: { id: string; data: CreateDeviceRequest }) =>
			updateDevice(vars.id, vars.data),
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: ["devices"] });
			closeModal();
		},
		onError: (err) => alert(err.message),
	});

	const deleteMutation = useMutation({
		mutationFn: deleteDevice,
		onSuccess: () => {
			queryClient.invalidateQueries({ queryKey: ["devices"] });
		},
		onError: (err) => alert(err.message),
	});

	// --- Handlers ---
	const openCreateModal = () => {
		setEditingId(null);
		setFormData({ fingerprint: "", classroomId: "" });
		setIsModalOpen(true);
	};

	const openEditModal = (device: (typeof devices)[0]) => {
		setEditingId(device.id);
		setFormData({
			fingerprint: device.fingerprint,
			classroomId: device.classroom?.id || "",
		});
		setIsModalOpen(true);
	};

	const closeModal = () => {
		setIsModalOpen(false);
		setEditingId(null);
	};

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();
		if (editingId) {
			updateMutation.mutate({ id: editingId, data: formData });
		} else {
			createMutation.mutate(formData);
		}
	};

	const handleDelete = (id: string) => {
		if (confirm("Are you sure you want to delete this device?")) {
			deleteMutation.mutate(id);
		}
	};

	const modalFingerprintId = useId();
	const modalClassroomId = useId();

	return (
		<div className="min-h-screen bg-slate-100 p-4 md:p-8 font-sans">
			<div className="max-w-5xl mx-auto">
				{/* HEADER */}
				<div className="flex flex-col md:flex-row justify-between items-end mb-8 border-b-8 border-black pb-4 gap-4">
					<div>
						<h1 className="text-4xl md:text-6xl font-black text-gray-900 uppercase tracking-tighter leading-none">
							Manage
							<br />
							Devices
						</h1>
					</div>
					<BlockyButton onClick={openCreateModal} variant="primary">
						+ Add Device
					</BlockyButton>
				</div>

				{/* TABLE */}
				<BlockyCard className="p-0 overflow-hidden bg-white">
					{loadingDevices ? (
						<div className="p-8 text-center font-bold">Loading devices...</div>
					) : devices.length === 0 ? (
						<div className="p-8 text-center text-gray-500 font-bold">
							No devices found.
						</div>
					) : (
						<div className="overflow-x-auto">
							<table className="w-full text-left border-collapse">
								<thead>
									<tr className="bg-black text-white uppercase text font-mono">
										<th className="p-4 border-r border-gray-700">
											Fingerprint (Base64)
										</th>
										<th className="p-4 border-r border-gray-700">Classroom</th>
										<th className="p-4 text-right">Actions</th>
									</tr>
								</thead>
								<tbody>
									{sortedDevices.map((device) => (
										<tr
											key={device.id}
											className="border-b-2 border-black transition-colors group"
										>
											<td className="p-4 font-mono font-bold">
												{device.fingerprint}
											</td>
											<td className="p-4 font-bold uppercase">
												{device.classroom?.name || "Unassigned"}
											</td>
											<td className="p-4 text-right flex justify-end gap-2">
												<BlockyButton
													variant="neutral"
													onClick={() => openEditModal(device)}
												>
													Edit
												</BlockyButton>
												<BlockyButton
													className="bg-red-500 text-white hover:bg-red-600"
													onClick={() => handleDelete(device.id)}
												>
													Delete
												</BlockyButton>
											</td>
										</tr>
									))}
								</tbody>
							</table>
						</div>
					)}
				</BlockyCard>

				<BlockyModal
					isOpen={isModalOpen}
					onClose={closeModal}
					title={editingId ? "Edit Device" : "New Device"}
				>
					<form onSubmit={handleSubmit} className="flex flex-col gap-4">
						{/* Fingerprint Input */}
						<div className="flex flex-col gap-1">
							<label
								htmlFor={modalFingerprintId}
								className="text-sm font-bold uppercase"
							>
								Fingerprint (Base64)
							</label>
							<input
								id={modalFingerprintId}
								type="text"
								required
								className="border-2 border-black p-2 font-mono focus:bg-blue-50 focus:outline-none"
								placeholder="e.g. dGVzdA=="
								value={formData.fingerprint}
								onChange={(e) =>
									setFormData({
										...formData,
										fingerprint: e.target.value,
									})
								}
							/>
							<p className="text-xs text-gray-500 font-mono">Must be unique.</p>
						</div>

						{/* Classroom Select */}
						<div className="flex flex-col gap-1">
							<label
								htmlFor={modalClassroomId}
								className="text-sm font-bold uppercase"
							>
								Classroom
							</label>
							<select
								id={modalClassroomId}
								required
								className="border-2 border-black p-2 font-bold focus:bg-blue-50 focus:outline-none"
								value={formData.classroomId}
								onChange={(e) =>
									setFormData({
										...formData,
										classroomId: e.target.value,
									})
								}
							>
								<option value="" disabled>
									Select a classroom...
								</option>
								{classrooms.map((c) => (
									<option key={c.id} value={c.id}>
										{c.name}
									</option>
								))}
							</select>
						</div>

						{/* Actions */}
						<div className="flex justify-end gap-4 mt-4 pt-4 border-t-2 border-gray-100">
							<BlockyButton variant="danger" onClick={closeModal}>
								Cancel
							</BlockyButton>
							<BlockyButton
								variant="success"
								type="submit"
								disabled={createMutation.isPending || updateMutation.isPending}
							>
								{editingId ? "Save Changes" : "Create Device"}
							</BlockyButton>
						</div>
					</form>
				</BlockyModal>
			</div>
		</div>
	);
}
