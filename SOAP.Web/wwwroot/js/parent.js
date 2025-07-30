// Parent Portal JavaScript

$(document).ready(function() {
    // Initialize parent portal functionality
    initializeParentPortal();
});

function initializeParentPortal() {
    // KCPE Number Verification
    initializeKcpeVerification();
    
    // Document Upload
    initializeDocumentUpload();
    
    // Form Validation
    initializeFormValidation();
    
    // Application Status Updates
    initializeStatusUpdates();
}

function initializeKcpeVerification() {
    $('#kcpe-verify-btn').on('click', function() {
        const kcpeNumber = $('#kcpe-number').val();
        
        if (!kcpeNumber || kcpeNumber.length !== 11) {
            showAlert('Please enter a valid 11-digit KCPE index number', 'warning');
            return;
        }
        
        verifyKcpeNumber(kcpeNumber);
    });
    
    $('#kcpe-number').on('input', function() {
        const value = $(this).val().replace(/\D/g, '');
        $(this).val(value);
        
        if (value.length === 11) {
            $('#kcpe-verify-btn').prop('disabled', false);
        } else {
            $('#kcpe-verify-btn').prop('disabled', true);
        }
    });
}

function verifyKcpeNumber(kcpeNumber) {
    showLoading('#kcpe-verify-btn');
    
    $.ajax({
        url: '/api/verify-kcpe',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(kcpeNumber),
        success: function(response) {
            hideLoading('#kcpe-verify-btn');
            
            if (response.valid) {
                showAlert(`Student found: ${response.studentName}`, 'success');
                $('#student-name').val(response.studentName);
                $('#kcpe-verification').hide();
                $('#application-form').show();
            } else {
                showAlert('KCPE number not found in our records. Please contact the school.', 'danger');
            }
        },
        error: function() {
            hideLoading('#kcpe-verify-btn');
            showAlert('Error verifying KCPE number. Please try again.', 'danger');
        }
    });
}

function initializeDocumentUpload() {
    // Drag and drop functionality
    $('.document-upload-area').on('dragover', function(e) {
        e.preventDefault();
        $(this).addClass('dragover');
    });
    
    $('.document-upload-area').on('dragleave', function(e) {
        e.preventDefault();
        $(this).removeClass('dragover');
    });
    
    $('.document-upload-area').on('drop', function(e) {
        e.preventDefault();
        $(this).removeClass('dragover');
        
        const files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            handleFileUpload(files[0], $(this).data('document-type'));
        }
    });
    
    // File input change
    $('.document-file-input').on('change', function() {
        const file = this.files[0];
        const documentType = $(this).data('document-type');
        
        if (file) {
            handleFileUpload(file, documentType);
        }
    });
}

function handleFileUpload(file, documentType) {
    // Validate file
    if (!validateFile(file)) {
        return;
    }
    
    const formData = new FormData();
    formData.append('DocumentFile', file);
    formData.append('DocumentType', documentType);
    formData.append('ApplicationId', $('#application-id').val());
    
    // Show upload progress
    showUploadProgress(documentType);
    
    $.ajax({
        url: '/Parent/Document/Upload',
        method: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        xhr: function() {
            const xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener('progress', function(e) {
                if (e.lengthComputable) {
                    const percentComplete = (e.loaded / e.total) * 100;
                    updateUploadProgress(documentType, percentComplete);
                }
            });
            return xhr;
        },
        success: function(response) {
            hideUploadProgress(documentType);
            showAlert('Document uploaded successfully!', 'success');
            updateDocumentStatus(documentType, 'uploaded');
        },
        error: function() {
            hideUploadProgress(documentType);
            showAlert('Error uploading document. Please try again.', 'danger');
        }
    });
}

function validateFile(file) {
    const allowedTypes = ['application/pdf', 'image/jpeg', 'image/jpg', 'image/png'];
    const maxSize = 2 * 1024 * 1024; // 2MB
    
    if (!allowedTypes.includes(file.type)) {
        showAlert('Please upload a PDF, JPG, or PNG file.', 'warning');
        return false;
    }
    
    if (file.size > maxSize) {
        showAlert('File size must be less than 2MB.', 'warning');
        return false;
    }
    
    return true;
}

function initializeFormValidation() {
    // Phone number formatting
    $('.phone-input').on('input', function() {
        let value = $(this).val().replace(/\D/g, '');
        
        if (value.startsWith('0') && value.length === 10) {
            // Format as 0XXX XXX XXX
            value = value.replace(/(\d{4})(\d{3})(\d{3})/, '$1 $2 $3');
        } else if (value.startsWith('254') && value.length === 12) {
            // Format as 254 XXX XXX XXX
            value = value.replace(/(\d{3})(\d{3})(\d{3})(\d{3})/, '$1 $2 $3 $4');
        }
        
        $(this).val(value);
    });
    
    // Real-time validation
    $('.form-control').on('blur', function() {
        validateField($(this));
    });
}

function validateField(field) {
    const value = field.val().trim();
    const fieldName = field.attr('name');
    let isValid = true;
    let message = '';
    
    // Required field validation
    if (field.prop('required') && !value) {
        isValid = false;
        message = 'This field is required.';
    }
    
    // Specific field validations
    switch (fieldName) {
        case 'ParentPhone':
        case 'EmergencyContact':
            if (value && !isValidPhoneNumber(value)) {
                isValid = false;
                message = 'Please enter a valid phone number.';
            }
            break;
        case 'StudentAge':
            const age = parseInt(value);
            if (value && (age < 13 || age > 20)) {
                isValid = false;
                message = 'Student age must be between 13 and 20 years.';
            }
            break;
    }
    
    // Update field appearance
    if (isValid) {
        field.removeClass('is-invalid').addClass('is-valid');
        field.siblings('.invalid-feedback').hide();
    } else {
        field.removeClass('is-valid').addClass('is-invalid');
        field.siblings('.invalid-feedback').text(message).show();
    }
    
    return isValid;
}

function isValidPhoneNumber(phone) {
    const cleaned = phone.replace(/\s/g, '');
    return /^(0\d{9}|254\d{9})$/.test(cleaned);
}

function initializeStatusUpdates() {
    // Poll for status updates every 30 seconds
    if ($('#application-status').length > 0) {
        setInterval(checkApplicationStatus, 30000);
    }
}

function checkApplicationStatus() {
    const applicationId = $('#application-id').val();
    
    if (!applicationId) return;
    
    $.ajax({
        url: `/api/application/${applicationId}/status`,
        method: 'GET',
        success: function(response) {
            if (response.status !== $('#current-status').text()) {
                updateApplicationStatus(response.status);
                showAlert(`Application status updated to: ${response.status}`, 'info');
            }
        },
        error: function() {
            console.log('Error checking application status');
        }
    });
}

function updateApplicationStatus(status) {
    $('#current-status').text(status);
    
    const statusIcon = $('#status-icon');
    const statusCard = $('.application-status-card');
    
    // Update status styling
    statusIcon.removeClass('pending approved rejected');
    statusCard.removeClass('border-warning border-success border-danger');
    
    switch (status.toLowerCase()) {
        case 'approved':
            statusIcon.addClass('approved');
            statusCard.addClass('border-success');
            break;
        case 'rejected':
            statusIcon.addClass('rejected');
            statusCard.addClass('border-danger');
            break;
        default:
            statusIcon.addClass('pending');
            statusCard.addClass('border-warning');
    }
}

// Utility functions
function showAlert(message, type) {
    const alertClass = `alert-${type}-custom`;
    const alert = $(`
        <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `);
    
    $('#alerts-container').append(alert);
    
    // Auto-dismiss after 5 seconds
    setTimeout(() => {
        alert.alert('close');
    }, 5000);
}

function showLoading(selector) {
    const button = $(selector);
    button.prop('disabled', true);
    button.html('<span class="spinner-border spinner-border-sm me-2"></span>Loading...');
}

function hideLoading(selector, originalText = 'Submit') {
    const button = $(selector);
    button.prop('disabled', false);
    button.html(originalText);
}

function showUploadProgress(documentType) {
    const progressContainer = $(`#${documentType}-progress`);
    progressContainer.show();
    progressContainer.find('.progress-bar').css('width', '0%');
}

function updateUploadProgress(documentType, percent) {
    const progressBar = $(`#${documentType}-progress .progress-bar`);
    progressBar.css('width', `${percent}%`);
    progressBar.text(`${Math.round(percent)}%`);
}

function hideUploadProgress(documentType) {
    const progressContainer = $(`#${documentType}-progress`);
    progressContainer.hide();
}

function updateDocumentStatus(documentType, status) {
    const statusElement = $(`#${documentType}-status`);
    statusElement.removeClass('status-pending status-uploaded status-verified status-rejected');
    statusElement.addClass(`status-${status}`);
    statusElement.text(status.charAt(0).toUpperCase() + status.slice(1));
}