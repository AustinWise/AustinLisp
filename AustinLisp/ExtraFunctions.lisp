(defun second (l) '(car (cdr l)))
(defun third (l) '(car (cdr (cdr l))))
(defun reverse-inner (src new) '(if src (reverse-inner (cdr src) (cons (car src) new)) new))
(defun reverse (l) '(reverse-inner l nil))
