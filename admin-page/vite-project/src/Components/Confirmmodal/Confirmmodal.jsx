import PropTypes from 'prop-types';
import './Confirmmodal.css'

 

 
  
const ConfirmationModal = ({ isOpen, onClose, onConfirm }) => {
    if (!isOpen) return null;

    return (
        <div className="modal-backdrop1">
            <div className="modal-content1">
                <h2>Confirm Changes</h2>
                <p>Are you sure you want to modify the selected products?</p>
                <button onClick={onConfirm} className="confirm-button1">Yes</button>
                <button onClick={onClose} className="cancel-button1">No</button>
            </div>
        </div>
    );
};
ConfirmationModal.propTypes = {
    isOpen: PropTypes.bool.isRequired,
    onClose: PropTypes.func.isRequired,
    onConfirm: PropTypes.func.isRequired,
    children: PropTypes.node
  };

export default ConfirmationModal;
