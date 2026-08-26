export function Modal({ title, children, onClose }) {
  return (
    <div className="overlay" onClick={onClose}>
      <div className="modal" onClick={(event) => event.stopPropagation()}>
        <div className="modal-head">
          <strong>{title}</strong>
          <button type="button" onClick={onClose}>Закрыть</button>
        </div>
        {children}
      </div>
    </div>
  );
}
