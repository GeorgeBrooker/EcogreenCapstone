import PropTypes from 'prop-types';
import './Modal.css'

 
  

const Modal = ({ isOpen, children, onClose }) => {
    if (!isOpen) return null;

    return (
        <div className="modal-backdrop">
        <div className="modal-content">
          <button onClick={onClose} className="modal-close-button">X</button>
          {children}
        </div>
      </div>
    );
};
Modal.propTypes = {
    isOpen: PropTypes.bool.isRequired,
    onClose: PropTypes.func.isRequired,
    children: PropTypes.node
  };

export default Modal;