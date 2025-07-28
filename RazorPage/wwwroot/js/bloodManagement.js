document.addEventListener('DOMContentLoaded', function () {
    // Handle blood type modal
    const editBloodTypeModal = document.getElementById('editBloodTypeModal');
    if (editBloodTypeModal) {
        editBloodTypeModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const bloodTypeId = button.getAttribute('data-id');
            const modalTitle = editBloodTypeModal.querySelector('.modal-title');
            const bloodTypeRow = button.closest('tr');
            const bloodType = bloodTypeRow.querySelector('td:first-child').textContent.trim();
            const availableUnits = bloodTypeRow.querySelector('td:nth-child(3)').textContent.trim();
            
            modalTitle.textContent = `Edit ${bloodType} Inventory`;
            
            // Set form values
            editBloodTypeModal.querySelector('#bloodTypeId').value = bloodTypeId;
            editBloodTypeModal.querySelector('#availableUnits').value = availableUnits
        });
    }

    // Handle component modal
    const editComponentModal = document.getElementById('editComponentModal');
    if (editComponentModal) {
        editComponentModal.addEventListener('show.bs.modal', function (event) {
            const button = event.relatedTarget;
            const componentId = button.getAttribute('data-id');
            const modalTitle = editComponentModal.querySelector('.modal-title');
            const componentRow = button.closest('tr');
            const componentName = componentRow.querySelector('td:first-child').textContent.trim();
            const availableUnits = componentRow.querySelector('td:nth-child(3)').textContent.trim();
            
            modalTitle.textContent = `Edit ${componentName} Inventory`;
            
            // Set form values
            editComponentModal.querySelector('#componentId').value = componentId;
            editComponentModal.querySelector('#availableUnits').value = availableUnits
        });
    }

    // Handle form submissions
    const updateBloodTypeForm = document.getElementById('updateBloodTypeForm');
    if (updateBloodTypeForm) {
        updateBloodTypeForm.addEventListener('submit', async function (event) {
            event.preventDefault();
            
            try {
                const formData = new FormData(updateBloodTypeForm);
                const response = await fetch('?handler=UpdateBloodType', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                });

                if (response.ok) {
                    window.location.reload();
                } else {
                    alert('Failed to update blood type inventory');
                }
            } catch (error) {
                console.error('Error:', error);
                alert('An error occurred while updating blood type inventory');
            }
        });
    }

    const updateComponentForm = document.getElementById('updateComponentForm');
    if (updateComponentForm) {
        updateComponentForm.addEventListener('submit', async function (event) {
            event.preventDefault();
            
            try {
                const formData = new FormData(updateComponentForm);
                const response = await fetch('?handler=UpdateComponent', {
                    method: 'POST',
                    body: formData,
                    headers: {
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    }
                });

                if (response.ok) {
                    window.location.reload();
                } else {
                    alert('Failed to update component inventory');
                }
            } catch (error) {
                console.error('Error:', error);
                alert('An error occurred while updating component inventory');
            }
        });
    }
});