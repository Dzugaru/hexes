module frontendMock;
import interfacing;
import enums;

extern(C) EntityHandle createEntityMock(EntityClass objClass, int objType)
{
	return EntityHandle(objClass, 0);
}

void setup()
{
	interfacing.cb.setBlackholes();
	interfacing.cb.createEntity = &createEntityMock;
}
