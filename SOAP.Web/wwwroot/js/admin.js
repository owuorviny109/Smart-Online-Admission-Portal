// Admin Dashboard JavaScript

$(document).ready(function() {
    // Initialize admin dashboard functionality
    initializeAdminDashboard();
});

function initializeAdminDashboard() {
    // Initialize DataTables
    initializeDataTables();
    
    // Initialize application review
    initializeApplicationReview();
    
    // Initialize bulk actions
    initializeBulkActions();
    
    // Initialize search and filters
    initializeSearchFilters();
    
    // Initialize charts
    initializeCharts();
}

function initializeDataTables() {
    if ($('#applications-table').length > 0) {
        $('#applications-table').DataTable({
            responsive: true,
            pageLength: 25,
            order: [[4, 'desc']], // Order by created date
            columnDefs: [
                { orderable: false, targets: [0, 5] }, // Checkbox and actions columns
                { searchable: false, targets: [0, 5] }
            ],
            language: {
                search: "Search applications:",
                lengthMenu: "Show _MENU_ applications per page",
                info: "Showing _START_ to _END_ of _TOTAL_ applications",
                paginate: {
                    first: "First",
                    last: "Last",
                    next: "Next",
                    previous: "Previous"
                }
            }
        });
    }
    
    if ($('#students-table').length > 0) {
        $('#students-table').DataTable({
            responsive: true,
            pageLength: 50,
            order: [[1, 'asc']], // Order by student name
            language: {
                search: "Search students:",
                lengthMenu: "Show _MENU_ students per page",
                info: "Showing _START_ to _END_ of _TOTAL_ students"
            }
        });
    }
}

function initializeApplicationReview() {
    // Document viewer
    $('.document-view-btn').on('click', function(e) {
        e.preventDefault();
        const documentId = $(this).data('document-id');
        const documentType = $(this).data('document-type');
        
        viewDocument(documentId, documentType);
    });
    
    // Document approval/rejection
    $('.document-approve-btn').on('click', function() {
        const documentId = $(this).data('document-id');
        updateDocumentStatus(documentId, 'Verified', '');
    });
    
    $('.document-reject-btn').on('click', function() {
        const documentId = $(this).data('document-id');
        const feedback = prompt('Please provide feedback for rejection:');
        
        if (feedback) {
            updateDocumentStatus(documentId, 'Rejected', feedback);
        }
    });
    
    // Application decision
    $('#approve-application-btn').on('click', function() {
        const applicationId = $(this).data('application-id');
        const comments = $('#admin-comments').val();
        
        approveApplication(applicationId, comments);
    });
    
    $('#reject-application-btn').on('click', function() {
        const applicationId = $(this).data('application-id');
        const reason = $('#rejection-reason').val();
        
        if (!reason.trim()) {
            showAlert('Please provide a reason for rejection.', 'warning');
            return;
        }
        
        rejectApplication(applicationId, reason);
    });
}

function viewDocument(documentId, documentType) {
    // Open document in modal or new tab
    const url = `/Admin/Document/View/${documentId}`;
    
    // Create modal for document viewing
    const modal = $(`
        <div class="modal fade" id="documentModal" tabindex="-1">
            <div class="modal-dialog modal-xl">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${documentType}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <iframe src="${url}" width="100%" height="600px" frameborder="0"></iframe>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Close</button>
                        <a href="${url}" target="_blank" class="btn btn-primary">Open in New Tab</a>
                    </div>
                </div>
            </div>
        </div>
    `);
    
    $('body').append(modal);
    $('#documentModal').modal('show');
    
    // Remove modal when hidden
    $('#documentModal').on('hidden.bs.modal', function() {
        $(this).remove();
    });
}

function updateDocumentStatus(documentId, status, feedback) {
    showLoading(`#document-${documentId}-actions`);
    
    $.ajax({
        url: '/Admin/Document/UpdateStatus',
        method: 'POST',
        data: {
            documentId: documentId,
            status: status,
            feedback: feedback
        },
        success: function(response) {
            hideLoading(`#document-${documentId}-actions`);
            
            if (response.success) {
                showAlert(`Document ${status.toLowerCase()} successfully!`, 'success');
                updateDocumentStatusUI(documentId, status, feedback);
            } else {
                showAlert('Error updating document status.', 'danger');
            }
        },
        error: function() {
            hideLoading(`#document-${documentId}-actions`);
            showAlert('Error updating document status.', 'danger');
        }
    });
}

function approveApplication(applicationId, comments) {
    if (!confirm('Are you sure you want to approve this application?')) {
        return;
    }
    
    showLoading('#approve-application-btn');
    
    $.ajax({
        url: '/Admin/Application/Approve',
        method: 'POST',
        data: {
            id: applicationId,
            comments: comments
        },
        success: function(response) {
            hideLoading('#approve-application-btn');
            
            if (response.success) {
                showAlert('Application approved successfully! SMS notification sent to parent.', 'success');
                setTimeout(() => {
                    window.location.href = '/Admin/Application';
                }, 2000);
            } else {
                showAlert('Error approving application.', 'danger');
            }
        },
        error: function() {
            hideLoading('#approve-application-btn');
            showAlert('Error approving application.', 'danger');
        }
    });
}

function rejectApplication(applicationId, reason) {
    if (!confirm('Are you sure you want to reject this application?')) {
        return;
    }
    
    showLoading('#reject-application-btn');
    
    $.ajax({
        url: '/Admin/Application/Reject',
        method: 'POST',
        data: {
            id: applicationId,
            reason: reason
        },
        success: function(response) {
            hideLoading('#reject-application-btn');
            
            if (response.success) {
                showAlert('Application rejected. SMS notification sent to parent.', 'success');
                setTimeout(() => {
                    window.location.href = '/Admin/Application';
                }, 2000);
            } else {
                showAlert('Error rejecting application.', 'danger');
            }
        },
        error: function() {
            hideLoading('#reject-application-btn');
            showAlert('Error rejecting application.', 'danger');
        }
    });
}

function initializeBulkActions() {
    // Select all checkbox
    $('#select-all').on('change', function() {
        const isChecked = $(this).is(':checked');
        $('.application-checkbox').prop('checked', isChecked);
        updateBulkActionButtons();
    });
    
    // Individual checkboxes
    $('.application-checkbox').on('change', function() {
        updateBulkActionButtons();
        
        // Update select all checkbox
        const totalCheckboxes = $('.application-checkbox').length;
        const checkedCheckboxes = $('.application-checkbox:checked').length;
        
        $('#select-all').prop('indeterminate', checkedCheckboxes > 0 && checkedCheckboxes < totalCheckboxes);
        $('#select-all').prop('checked', checkedCheckboxes === totalCheckboxes);
    });
    
    // Bulk approve
    $('#bulk-approve-btn').on('click', function() {
        const selectedIds = getSelectedApplicationIds();
        
        if (selectedIds.length === 0) {
            showAlert('Please select applications to approve.', 'warning');
            return;
        }
        
        if (confirm(`Are you sure you want to approve ${selectedIds.length} applications?`)) {
            bulkApproveApplications(selectedIds);
        }
    });
    
    // Bulk reject
    $('#bulk-reject-btn').on('click', function() {
        const selectedIds = getSelectedApplicationIds();
        
        if (selectedIds.length === 0) {
            showAlert('Please select applications to reject.', 'warning');
            return;
        }
        
        const reason = prompt('Please provide a reason for bulk rejection:');
        if (reason && confirm(`Are you sure you want to reject ${selectedIds.length} applications?`)) {
            bulkRejectApplications(selectedIds, reason);
        }
    });
}

function updateBulkActionButtons() {
    const selectedCount = $('.application-checkbox:checked').length;
    
    if (selectedCount > 0) {
        $('#bulk-actions').show();
        $('#selected-count').text(selectedCount);
    } else {
        $('#bulk-actions').hide();
    }
}

function getSelectedApplicationIds() {
    const ids = [];
    $('.application-checkbox:checked').each(function() {
        ids.push($(this).val());
    });
    return ids;
}

function initializeSearchFilters() {
    // Status filter
    $('#status-filter').on('change', function() {
        const status = $(this).val();
        filterApplicationsByStatus(status);
    });
    
    // Date range filter
    $('#date-from, #date-to').on('change', function() {
        const dateFrom = $('#date-from').val();
        const dateTo = $('#date-to').val();
        filterApplicationsByDateRange(dateFrom, dateTo);
    });
    
    // Quick search
    $('#quick-search').on('input', debounce(function() {
        const searchTerm = $(this).val();
        quickSearchApplications(searchTerm);
    }, 300));
}

function filterApplicationsByStatus(status) {
    const table = $('#applications-table').DataTable();
    
    if (status === '') {
        table.column(3).search('').draw(); // Status column
    } else {
        table.column(3).search(status).draw();
    }
}

function filterApplicationsByDateRange(dateFrom, dateTo) {
    // Custom date range filtering for DataTables
    $.fn.dataTable.ext.search.push(function(settings, data, dataIndex) {
        if (settings.nTable.id !== 'applications-table') {
            return true;
        }
        
        const dateCreated = new Date(data[4]); // Created date column
        const minDate = dateFrom ? new Date(dateFrom) : null;
        const maxDate = dateTo ? new Date(dateTo) : null;
        
        if (minDate && dateCreated < minDate) {
            return false;
        }
        
        if (maxDate && dateCreated > maxDate) {
            return false;
        }
        
        return true;
    });
    
    $('#applications-table').DataTable().draw();
}

function quickSearchApplications(searchTerm) {
    $('#applications-table').DataTable().search(searchTerm).draw();
}

function initializeCharts() {
    // Initialize dashboard charts if Chart.js is available
    if (typeof Chart !== 'undefined') {
        initializeApplicationStatusChart();
        initializeDailyApplicationsChart();
    }
}

function initializeApplicationStatusChart() {
    const ctx = document.getElementById('statusChart');
    if (!ctx) return;
    
    // Get data from data attributes or AJAX call
    const data = {
        labels: ['Pending', 'Approved', 'Rejected', 'Incomplete'],
        datasets: [{
            data: [
                parseInt($('#pending-count').text()) || 0,
                parseInt($('#approved-count').text()) || 0,
                parseInt($('#rejected-count').text()) || 0,
                parseInt($('#incomplete-count').text()) || 0
            ],
            backgroundColor: [
                '#f59e0b',
                '#10b981',
                '#ef4444',
                '#6366f1'
            ]
        }]
    };
    
    new Chart(ctx, {
        type: 'doughnut',
        data: data,
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

function initializeDailyApplicationsChart() {
    const ctx = document.getElementById('dailyChart');
    if (!ctx) return;
    
    // This would typically fetch data from an API endpoint
    $.ajax({
        url: '/Admin/Analytics/DailyApplications',
        method: 'GET',
        success: function(response) {
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: response.labels,
                    datasets: [{
                        label: 'Applications per Day',
                        data: response.data,
                        borderColor: '#3b82f6',
                        backgroundColor: 'rgba(59, 130, 246, 0.1)',
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        }
    });
}

// Utility functions
function showAlert(message, type) {
    const alertClass = `alert-${type}`;
    const alert = $(`
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);
    
    $('#alerts-container').prepend(alert);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        alert.alert('close');
    }, 5000);
}

function showLoading(selector) {
    const element = $(selector);
    element.prop('disabled', true);
    
    if (element.is('button')) {
        element.data('original-text', element.html());
        element.html('<span class="spinner-border spinner-border-sm me-2"></span>Loading...');
    } else {
        element.append('<div class="loading-overlay"><span class="spinner-border"></span></div>');
    }
}

function hideLoading(selector) {
    const element = $(selector);
    element.prop('disabled', false);
    
    if (element.is('button')) {
        const originalText = element.data('original-text');
        if (originalText) {
            element.html(originalText);
        }
    } else {
        element.find('.loading-overlay').remove();
    }
}

function updateDocumentStatusUI(documentId, status, feedback) {
    const statusElement = $(`#document-${documentId}-status`);
    const actionsElement = $(`#document-${documentId}-actions`);
    
    // Update status badge
    statusElement.removeClass('badge-pending badge-uploaded badge-verified badge-rejected');
    statusElement.addClass(`badge-${status.toLowerCase()}`);
    statusElement.text(status);
    
    // Update actions based on status
    if (status === 'Verified' || status === 'Rejected') {
        actionsElement.html(`<span class="text-muted">Reviewed</span>`);
    }
    
    // Show feedback if rejected
    if (status === 'Rejected' && feedback) {
        const feedbackElement = $(`#document-${documentId}-feedback`);
        feedbackElement.text(feedback).show();
    }
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Export functions for external use
window.AdminDashboard = {
    showAlert,
    showLoading,
    hideLoading,
    viewDocument,
    updateDocumentStatus
};